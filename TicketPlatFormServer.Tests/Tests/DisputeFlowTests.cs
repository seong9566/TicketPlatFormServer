using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Dispute;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class DisputeFlowTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private int _userId;
    private string _userEmail = string.Empty;

    public DisputeFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();

        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);

        var user = await CreateUserWithFallbackAsync(seeder, conn);
        _userId = user.userId;
        _userEmail = user.email;

        TestAuthHelper.AddAuthHeader(_client, _userId, _userEmail);
    }

    public async Task DisposeAsync()
    {
        if (_userId > 0)
        {
            await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(
                "DELETE FROM user_profile WHERE user_id = @id; " +
                "DELETE FROM refresh_tokens WHERE user_id = @id; " +
                "DELETE FROM users WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", _userId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task DisputeFlow_GetMyDisputes_Returns200()
    {
        var response = await _client.GetAsync("/api/disputes?limit=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DisputeListRespDto>>();
        payload.Should().NotBeNull();
        payload!.StatusCode.Should().Be(200);
        payload.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();
        payload.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task DisputeFlow_CreateDispute_WithoutTransaction_Returns4xx()
    {
        var req = new CreateDisputeReqDto
        {
            TransactionId = 999_999_999L,   // 존재하지 않는 거래 ID
            TypeCode = "FAKE_TICKET",
            Description = "테스트 신고입니다. 존재하지 않는 거래 ID로 요청합니다."
        };

        var response = await _client.PostAsJsonAsync("/api/disputes", req);

        ((int)response.StatusCode).Should().BeInRange(400, 499);

    }

    private static async Task<(int userId, string email, string password)> CreateUserWithFallbackAsync(
        TestDataSeeder seeder,
        MySqlConnection conn)
    {
        try
        {
            return await seeder.CreateUserAsync(role: "user");
        }
        catch (MySqlException ex) when (ex.Message.Contains("Unknown column 'role'", StringComparison.OrdinalIgnoreCase))
        {
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            var email = $"test_{Guid.NewGuid():N}@test.com";
            const string rawPassword = "Test1234!@#";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            var phone = $"010{Random.Shared.Next(10000000, 99999999)}";

            await using var insertUserCmd = new MySqlCommand(@"
                INSERT INTO users (email, password_hash, phone, provider_id, role_id, created_at, is_deleted)
                VALUES (@email, @hash, @phone, @providerId, @roleId, NOW(), 0);", conn);
            insertUserCmd.Parameters.AddWithValue("@email", email);
            insertUserCmd.Parameters.AddWithValue("@hash", hashedPassword);
            insertUserCmd.Parameters.AddWithValue("@phone", phone);
            insertUserCmd.Parameters.AddWithValue("@providerId", 1);
            insertUserCmd.Parameters.AddWithValue("@roleId", 2);
            await insertUserCmd.ExecuteNonQueryAsync();

            var userId = (int)insertUserCmd.LastInsertedId;

            await using var profileCmd = new MySqlCommand(@"
                INSERT INTO user_profile (user_id, nickname)
                VALUES (@userId, @nickname);", conn);
            profileCmd.Parameters.AddWithValue("@userId", userId);
            profileCmd.Parameters.AddWithValue("@nickname", $"TestUser{userId}");
            await profileCmd.ExecuteNonQueryAsync();

            return (userId, email, rawPassword);
        }
    }
}
