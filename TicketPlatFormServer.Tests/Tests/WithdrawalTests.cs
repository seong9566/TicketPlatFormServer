using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Withdrawal;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class WithdrawalTests : IAsyncLifetime
{
    private readonly WithdrawalWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private int _userId;
    private string _email = string.Empty;

    public WithdrawalTests()
    {
        _factory = new WithdrawalWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);
        (_userId, _email, _) = await seeder.CreateUserAsync();
        TestAuthHelper.AddAuthHeader(_client, _userId, _email);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await TestDbManager.CleanupAsync();
        }
        catch (MySqlException)
        {
            await SafeCleanupTestUsersAsync();
        }
        finally
        {
            await _factory.DisposeAsync();
        }
    }

    private static async Task SafeCleanupTestUsersAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();
        var commands = new[]
        {
            "DELETE up FROM user_profile up INNER JOIN users u ON up.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE rt FROM refresh_token rt INNER JOIN users u ON rt.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
            "DELETE FROM users WHERE email LIKE 'test_%@test.com'"
        };
        foreach (var sql in commands)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            try { await cmd.ExecuteNonQueryAsync(); } catch { /* ignore */ }
        }
    }

    [Fact]
    public async Task Withdrawal_GetHistory_Returns200()
    {
        // 신규 유저 → 출금 내역 없음 (빈 리스트도 OK)
        var response = await _client.GetAsync("/api/withdrawal/history?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<WithdrawalListResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeNull();
        result.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Withdrawal_Request_WithoutBankAccount_Returns4xx()
    {
        // 계좌 미등록 상태에서 출금 요청 → 400/404 예상
        var request = new WithdrawalRequestDto
        {
            Amount = 10000,
            BankAccountId = null
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/withdrawal")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

        var response = await _client.SendAsync(httpRequest);

        ((int)response.StatusCode).Should().BeInRange(400, 499);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.StatusCode.Should().Be((int)response.StatusCode);
    }

    // Development 환경 사용: GlobalExceptionMiddleware가 AppException을 정상 처리하도록
    private sealed class WithdrawalWebApplicationFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
        }
    }
}
