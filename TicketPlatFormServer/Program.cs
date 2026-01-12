using System.Text;
using Amazon.S3;
using Amazon;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Hubs;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.Chat;
using TicketPlatFormServer.Repository.Events;
using TicketPlatFormServer.Repository.Favorite;
using TicketPlatFormServer.Repository.Home;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Repository.Token;
using TicketPlatFormServer.Repository.Users;
using TicketPlatFormServer.Services.BackgroundServices;
using TicketPlatFormServer.Services.Chat;
using TicketPlatFormServer.Services.Event;
using TicketPlatFormServer.Services.Favorite;
using TicketPlatFormServer.Services.FileUpload;
using TicketPlatFormServer.Services.Home;
using TicketPlatFormServer.Services.Ticket;
using TicketPlatFormServer.Services.Token;
using TicketPlatFormServer.Services.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Swagger 서비스 등록
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

#region AWS S3 설정
var awsS3Settings = new AwsS3Settings();
builder.Configuration.GetSection("AwsS3Settings").Bind(awsS3Settings);
builder.Services.AddSingleton(awsS3Settings);

// AWS S3 클라이언트 등록
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var settings = sp.GetRequiredService<AwsS3Settings>();
    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
    };
    return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
});
#endregion

#region Chat 설정
var chatSettings = new ChatSettings();
builder.Configuration.GetSection("ChatSettings").Bind(chatSettings);
builder.Services.AddSingleton(chatSettings);
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
                mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null); // 재시도 3회, 10초 간격
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

// Chat 서비스
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Background 서비스
builder.Services.AddHostedService<ChatCleanupService>();

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