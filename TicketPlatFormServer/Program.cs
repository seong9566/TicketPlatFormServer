using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.EventRepo;
using TicketPlatFormServer.Repository.Home;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Repository.Users;
using TicketPlatFormServer.Services.Event;
using TicketPlatFormServer.Services.Home;
using TicketPlatFormServer.Services.Ticket;
using TicketPlatFormServer.Services.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Swagger 서비스 등록
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
            options.LogTo(Console.WriteLine, new[]
            {
                DbLoggerCategory.Database.Command.Name,
                DbLoggerCategory.Database.Transaction.Name,
                DbLoggerCategory.Database.Connection.Name
            }, LogLevel.Warning);

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();