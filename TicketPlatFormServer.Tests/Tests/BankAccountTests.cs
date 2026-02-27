using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.BankAccount;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class BankAccountTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private int _userId;
    private string _email = string.Empty;

    public BankAccountTests(TestWebApplicationFactory factory)
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
        await CleanupBankAccountsAsync();
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

    private static async Task CleanupBankAccountsAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();
        try
        {
            await using var cmd = new MySqlCommand(
                "DELETE ba FROM bank_account ba INNER JOIN users u ON ba.user_id = u.id WHERE u.email LIKE 'test_%@test.com'",
                conn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // 테이블 구조가 다를 경우 무시
        }
    }

    private static RegisterBankAccountRequestDto CreateTestBankAccountDto() => new()
    {
        BankName = "테스트은행",
        BankCode = "001",
        AccountNumber = $"110{new Random().Next(100000000, 999999999)}",
        AccountHolder = "홍길동"
    };

    [Fact]
    public async Task BankAccount_Register_Returns201()
    {
        var req = CreateTestBankAccountDto();

        var response = await _client.PostAsJsonAsync("/api/bank-account", req);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BankAccountResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.BankName.Should().Be(req.BankName);
        result.Data.AccountHolder.Should().Be(req.AccountHolder);
    }

    [Fact]
    public async Task BankAccount_GetMe_Returns200()
    {
        // 먼저 계좌 등록
        var req = CreateTestBankAccountDto();
        var registerResponse = await _client.PostAsJsonAsync("/api/bank-account", req);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 등록된 계좌 조회
        var response = await _client.GetAsync("/api/bank-account/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BankAccountResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.BankName.Should().Be(req.BankName);
    }

    [Fact]
    public async Task BankAccount_Delete_Returns200()
    {
        // 먼저 계좌 등록
        var req = CreateTestBankAccountDto();
        var registerResponse = await _client.PostAsJsonAsync("/api/bank-account", req);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 등록된 계좌 삭제
        var response = await _client.DeleteAsync("/api/bank-account");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().NotBeNullOrWhiteSpace();
    }
}
