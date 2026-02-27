using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using TicketPlatFormServer;
using TicketPlatFormServer.Repository;

namespace TicketPlatFormServer.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestConnectionString =
        "Server=127.0.0.1;Port=3306;Database=TicketPlatFormDB_Test;User=root;Password=stecdev1234!;SslMode=None;AllowPublicKeyRetrieval=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Encryption:MasterKey"] = "TicketPlatform-AES256-Encryption-Master-Key-2026-Secure-Payment-Data-Protection"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();

            var dbContextDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TicketContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextDescriptor2 = services.FirstOrDefault(
                d => d.ServiceType == typeof(TicketContext));
            if (dbContextDescriptor2 != null)
            {
                services.Remove(dbContextDescriptor2);
            }

            services.AddDbContext<TicketContext>(options =>
            {
                options.UseMySql(
                    TestConnectionString,
                    ServerVersion.AutoDetect(TestConnectionString),
                    mySqlOptions =>
                    {
                        mySqlOptions.CommandTimeout(60);
                        mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });
            });

            var dbConnDescriptors = services
                .Where(d => d.ServiceType == typeof(System.Data.IDbConnection))
                .ToList();

            foreach (var descriptor in dbConnDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddScoped<System.Data.IDbConnection>(_ =>
                new MySqlConnection(TestConnectionString));
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => base.DisposeAsync().AsTask();
}
