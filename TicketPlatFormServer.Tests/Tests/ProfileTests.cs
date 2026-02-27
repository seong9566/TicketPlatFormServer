using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.User;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class ProfileTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private int _userId;
    private string _email = string.Empty;
    private string _password = string.Empty;

    public ProfileTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);
        (_userId, _email, _password) = await seeder.CreateUserAsync();
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
    public async Task Profile_GetMyProfile_Returns200()
    {
        var response = await _client.GetAsync("/api/users/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(_userId);
        result.Data.Email.Should().Be(_email);
    }

    [Fact]
    public async Task Profile_UpdateProfile_Returns200()
    {
        // 닉네임: 2~20자, 영문/숫자/언더스코어 조합
        var newNickname = $"Nick_{new Random().Next(1000, 9999)}";

        using var form = new MultipartFormDataContent
        {
            { new StringContent(newNickname), "Nickname" },
            { new StringContent("Updated bio for E2E test"), "Bio" }
        };

        var response = await _client.PutAsync("/api/users/profile", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Profile_ChangePassword_WrongOldPassword_Returns4xx()
    {
        // 실제 비밀번호는 "Test1234!@#" 이지만 틀린 비밀번호로 변경 시도
        var body = new ChangePasswordReqDto
        {
            CurrentPassword = "WrongPassword999!",
            NewPassword = "NewPassword123!@"
        };

        var response = await _client.PutAsJsonAsync("/api/users/password", body);

        ((int)response.StatusCode).Should().BeInRange(400, 499);

        var content = await response.Content.ReadAsStringAsync();
        // 응답이 있는 경우에만 JSON 파싱 (빈 응답도 허용)
        if (!string.IsNullOrWhiteSpace(content))
        {
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.TryGetProperty("success", out var successProp);
            if (successProp.ValueKind == JsonValueKind.True)
            {
                // success가 true면 실패 (잘못된 비밀번호인데 성공하면 안 됨)
                false.Should().BeTrue("Wrong password should not succeed");
            }
        }
    }
}
