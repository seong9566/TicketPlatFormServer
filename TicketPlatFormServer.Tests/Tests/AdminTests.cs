using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Admin;
using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class AdminTests : IAsyncLifetime
{
    private readonly AdminWebApplicationFactory _factory;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _userClient;
    private readonly HttpClient _anonClient;
    private int _targetUserId;
    private string _targetEmail = string.Empty;

    public AdminTests()
    {
        var factory = new AdminWebApplicationFactory();
        _adminClient = factory.CreateClient();
        _userClient = factory.CreateClient();
        _anonClient = factory.CreateClient();
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);

        // 대상 유저 생성 (admin이 잔액 조회할 유저)
        (_targetUserId, _targetEmail, _) = await seeder.CreateUserAsync();

        // 일반 유저 생성 → _userClient에 토큰 설정
        var (userUserId, userEmail, _) = await seeder.CreateUserAsync();
        TestAuthHelper.AddAuthHeader(_userClient, userUserId, userEmail, "user");

        // Admin 토큰 → _adminClient에 설정
        var adminToken = TestAuthHelper.GenerateAdminToken(999, "admin@test.com");
        _adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        // _anonClient: 헤더 없음 (401 테스트용)
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
    public async Task Admin_GetUserBalance_WithAdminToken_Returns200()
    {
        // Admin 토큰으로 대상 유저 잔액 조회 → 200 OK
        var response = await _adminClient.GetAsync($"/api/admin/balance/{_targetUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BalanceResponseDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Available.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Admin_AdjustBalance_WithUserToken_Returns403()
    {
        // 일반 유저 토큰으로 admin API 호출 → 403 Forbidden
        var request = new AdminAdjustBalanceRequestDto
        {
            Amount = 1000,
            Reason = "테스트 조정"
        };

        var response = await _userClient.PostAsJsonAsync(
            $"/api/admin/balance/{_targetUserId}/adjust", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Admin_GetUserBalance_WithoutToken_Returns401()
    {
        // 토큰 없이 admin API 호출 → 401 Unauthorized
        var response = await _anonClient.GetAsync($"/api/admin/balance/{_targetUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Development 환경 사용: GlobalExceptionMiddleware가 AppException을 정상 처리하도록
    private sealed class AdminWebApplicationFactory : TestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
        }
    }
}
