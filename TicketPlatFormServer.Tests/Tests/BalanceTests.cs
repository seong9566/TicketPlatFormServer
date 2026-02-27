using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class BalanceTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private int _userId;
    private string _email = string.Empty;

    public BalanceTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
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
    public async Task Balance_GetMyBalance_Returns200()
    {
        // 신규 유저 → 잔액 0원
        var response = await _client.GetAsync("/api/balance");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BalanceResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Available.Should().BeGreaterThanOrEqualTo(0);
        result.Data.Pending.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Balance_GetHistory_Returns200()
    {
        // 신규 유저 → 빈 거래 내역
        var response = await _client.GetAsync("/api/balance/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BalanceHistoryResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeNull();
        result.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }
}
