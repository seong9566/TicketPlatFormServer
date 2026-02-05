using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Hubs;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.Chat;
using TicketPlatFormServer.Repository.Events;
using TicketPlatFormServer.Repository.Favorite;
using TicketPlatFormServer.Repository.Home;
using TicketPlatFormServer.Repository.Payment;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Repository.Token;
using TicketPlatFormServer.Repository.Transactions;
using TicketPlatFormServer.Repository.Users;
using TicketPlatFormServer.Repository.Sell;
using TicketPlatFormServer.Services.BackgroundServices;
using TicketPlatFormServer.Services.Chat;
using TicketPlatFormServer.Services.Event;
using TicketPlatFormServer.Services.Favorite;
using TicketPlatFormServer.Services.FileUpload;
using TicketPlatFormServer.Services.Home;
using TicketPlatFormServer.Services.Payment;
using TicketPlatFormServer.Services.Ticket;
using TicketPlatFormServer.Services.Token;
using TicketPlatFormServer.Services.User;
using TicketPlatFormServer.Services.Storage;
using TicketPlatFormServer.Services.Sell;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

EnvFileLoader.Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));
EnvFileLoader.Load(Path.Combine(builder.Environment.ContentRootPath, "db_connect.env"));

builder.Configuration.AddJsonFile(
    "appsettings.SupabaseStorage.json",
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Swagger 서비스 등록
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TicketPlatForm API",
        Version = "v1",
        Description = "티켓 플랫폼 API 문서"
    });

    // JWT Bearer 인증 스키마 추가
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    // 모든 API에 JWT 인증 요구사항 적용
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#region JWT 인증 설정
// JWT 설정 읽기
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// JWT 인증 미들웨어
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = jwtSettings.ValidateIssuer,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = jwtSettings.ValidateAudience,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = jwtSettings.ValidateLifetime,
            ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkew)
        };

        // SignalR을 위한 이벤트 설정
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (path.StartsWithSegments("/hubs"))
                {
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("[SignalR.Auth] access_token missing. Path={Path}, ConnectionId={ConnectionId}",
                            path, context.HttpContext.Connection.Id);
                    }
                }

                // SignalR Hub 경로에서 쿼리스트링으로 토큰 받기
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
#endregion

#region Chat 설정
var chatSettings = new ChatSettings();
builder.Configuration.GetSection("ChatSettings").Bind(chatSettings);
builder.Services.AddSingleton(chatSettings);
#endregion

#region Toss Payments 설정
var tossPaymentsSettings = new TossPaymentsSettings();
builder.Configuration.GetSection("TossPayments").Bind(tossPaymentsSettings);
builder.Services.AddSingleton(tossPaymentsSettings);

// Resilience settings for HttpClient (loaded in Supabase Storage section below)
// HttpClient for TossPaymentsService - will use resilienceSettings after it's loaded
#endregion

#region Supabase Storage 설정
var supabaseSettings = new SupabaseStorageSettings();
builder.Configuration.GetSection("SupabaseStorage").Bind(supabaseSettings);
builder.Services.AddSingleton(supabaseSettings);

var storageProviderSettings = new StorageProviderSettings();
builder.Configuration.GetSection("StorageProvider").Bind(storageProviderSettings);
builder.Services.AddSingleton(storageProviderSettings);

var resilienceSettings = new ResilienceSettings();
builder.Configuration.GetSection("Resilience").Bind(resilienceSettings);
builder.Services.AddSingleton(resilienceSettings);

// Memory Cache
builder.Services.AddMemoryCache();

// Signed URL Cache Service
builder.Services.AddScoped<ISignedUrlCacheService, SignedUrlCacheService>();

// HttpClient with Polly for SupabaseStorageUploader
builder.Services.AddHttpClient<SupabaseStorageUploader>()
    .AddPolicyHandler(GetRetryPolicy(resilienceSettings))
    .AddPolicyHandler(GetCircuitBreakerPolicy(resilienceSettings));

// IStorageUploader - Supabase로 설정
builder.Services.AddScoped<IStorageUploader>(sp => sp.GetRequiredService<SupabaseStorageUploader>());

// HttpClient for TossPaymentsService
builder.Services.AddHttpClient("TossPayments")
    .AddPolicyHandler(GetRetryPolicy(resilienceSettings))
    .AddPolicyHandler(GetCircuitBreakerPolicy(resilienceSettings));

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ResilienceSettings settings)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            settings.MaxRetryAttempts,
            retryAttempt => TimeSpan.FromMilliseconds(
                settings.InitialRetryDelayMs * Math.Pow(2, retryAttempt - 1)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ResilienceSettings settings)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            settings.CircuitBreakerFailureThreshold,
            TimeSpan.FromSeconds(settings.CircuitBreakerDurationSec));
}
#endregion

#region SignalR 설정
builder.Services.AddSignalR();
#endregion

#region DB 연결 설정
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    // EF Core
    builder.Services.AddDbContext<TicketContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
            mySqlOptions =>
            {
                // mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null); // 수동 트랜잭션과 충돌하여 비활성화
                mySqlOptions.CommandTimeout(60); // CommandTimeout 60초
                mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); // 복잡한 쿼리의 성능 향상을 위한 쿼리 분할 사용
            });

        // 개발 환경에서만 로깅 활성화
        if (builder.Environment.IsDevelopment())
        {
            // EF Core 쿼리 로깅은 ASP.NET Core의 기본 로깅 시스템(ILogger) 사용
            // appsettings.Development.json에서 로그 레벨 설정 가능
            options.EnableDetailedErrors();

            // 개발 환경에서만 민감 데이터 로깅 (보안: 프로덕션에서는 비활성화)
            options.EnableSensitiveDataLogging();
        }
    });
    
    // Dapper용 IDbConnection 등록 (Scoped)
    builder.Services.AddScoped<System.Data.IDbConnection>(sp => 
        new MySqlConnector.MySqlConnection(connectionString));
}
else
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is null or empty.");
}
#endregion

// ---------- 레이어 별 의존성 주입 -----------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionItemRepository, TransactionItemRepository>();
builder.Services.AddScoped<ITransactionHistoryRepository, TransactionHistoryRepository>();
builder.Services.AddScoped<TicketPlatFormServer.Services.Transaction.ITransactionService, TicketPlatFormServer.Services.Transaction.TransactionService>();

// Sell 서비스
builder.Services.AddScoped<ISellRepository, SellRepository>();
builder.Services.AddScoped<ISellService, SellService>();

// Chat 서비스
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Payment 서비스
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ITossPaymentsService, TossPaymentsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Encryption 서비스
builder.Services.AddSingleton<TicketPlatFormServer.Services.Common.EncryptionService>(sp =>
{
    var encryptionKey = builder.Configuration["Encryption:MasterKey"]
        ?? throw new InvalidOperationException("Encryption:MasterKey is not configured");
    return new TicketPlatFormServer.Services.Common.EncryptionService(encryptionKey);
});

// Background 서비스
builder.Services.AddHostedService<ChatCleanupService>();
builder.Services.AddHostedService<TransactionReservationCleanupService>();

var app = builder.Build();

// Exception 미들 웨어 추가 
// 자동으로 클라이언트의 요청에 대한 로직이 처리 미들웨어를 거치도록 됌.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) 
{
    // 개발환경일 경우에만 스웨거 실행
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        // 스웨거를 기본 경로로 
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketPlatForm API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// SignalR Hub 매핑
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
