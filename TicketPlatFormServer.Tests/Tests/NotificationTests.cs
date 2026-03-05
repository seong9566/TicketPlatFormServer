using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Notification;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class NotificationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly HttpClient _emptyUserClient;
    private readonly HttpClient _anonClient;
    private int _userId;
    private string _email = string.Empty;
    private int _emptyUserId;
    private string _emptyUserEmail = string.Empty;

    public NotificationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _emptyUserClient = factory.CreateClient();
        _anonClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);

        (_userId, _email, _) = await seeder.CreateUserAsync();
        (_emptyUserId, _emptyUserEmail, _) = await seeder.CreateUserAsync();

        TestAuthHelper.AddAuthHeader(_client, _userId, _email);
        TestAuthHelper.AddAuthHeader(_emptyUserClient, _emptyUserId, _emptyUserEmail);
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
            try { await cmd.ExecuteNonQueryAsync(); } catch { }
        }
    }

    [Fact]
    public async Task Notification_GetNotifications_ReturnsUserNotifications()
    {
        var firstId = await SeedNotificationAsync(_userId, "알림-1", false);
        var secondId = await SeedNotificationAsync(_userId, "알림-2", true);

        var response = await _client.GetAsync("/api/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<NotificationListRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Select(x => x.Id).Should().Contain(new long[] { firstId, secondId });
    }

    [Fact]
    public async Task Notification_ReadOne_MarksNotificationAsRead()
    {
        var notificationId = await SeedNotificationAsync(_userId, "읽음 테스트", false);

        var response = await _client.PutAsync($"/api/notifications/{notificationId}/read", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var (readFlag, readAt) = await GetReadStateAsync(notificationId);
        readFlag.Should().BeTrue();
        readAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Notification_ReadAll_MarksAllUnreadNotificationsAsRead()
    {
        await SeedNotificationAsync(_userId, "전체읽음-1", false);
        await SeedNotificationAsync(_userId, "전체읽음-2", false);
        await SeedNotificationAsync(_userId, "전체읽음-3", true);

        var response = await _client.PutAsync("/api/notifications/read-all", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReadAllRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedCount.Should().Be(2);

        var unreadCount = await GetUnreadCountAsync(_userId);
        unreadCount.Should().Be(0);
    }

    [Fact]
    public async Task Notification_GetNotifications_WithoutAuth_Returns401()
    {
        var response = await _anonClient.GetAsync("/api/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Notification_GetNotifications_EmptyList_ReturnsEmptyArray()
    {
        var response = await _emptyUserClient.GetAsync("/api/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<NotificationListRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.HasMore.Should().BeFalse();
        result.Data.NextCursor.Should().BeNull();
    }

    private static async Task<long> SeedNotificationAsync(long userId, string title, bool readFlag)
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        const string typeSql = "SELECT id FROM notification_types WHERE code = @code LIMIT 1";
        await using var typeCmd = new MySqlCommand(typeSql, conn);
        typeCmd.Parameters.AddWithValue("@code", "CHAT_MESSAGE");
        var typeObj = await typeCmd.ExecuteScalarAsync();
        typeObj.Should().NotBeNull();
        var typeId = Convert.ToInt64(typeObj);

        const string insertSql = @"
            INSERT INTO notifications (user_id, type_id, title, body, read_flag, read_at, data, created_at)
            VALUES (@userId, @typeId, @title, @body, @readFlag, @readAt, @data, UTC_TIMESTAMP())";

        await using var insertCmd = new MySqlCommand(insertSql, conn);
        insertCmd.Parameters.AddWithValue("@userId", userId);
        insertCmd.Parameters.AddWithValue("@typeId", typeId);
        insertCmd.Parameters.AddWithValue("@title", title);
        insertCmd.Parameters.AddWithValue("@body", $"{title} 내용");
        insertCmd.Parameters.AddWithValue("@readFlag", readFlag);
        insertCmd.Parameters.AddWithValue("@readAt", readFlag ? DateTime.UtcNow : null);
        insertCmd.Parameters.AddWithValue("@data", "{\"source\":\"e2e\"}");

        await insertCmd.ExecuteNonQueryAsync();
        return insertCmd.LastInsertedId;
    }

    private static async Task<(bool? readFlag, DateTime? readAt)> GetReadStateAsync(long notificationId)
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        const string sql = "SELECT read_flag, read_at FROM notifications WHERE id = @id LIMIT 1";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", notificationId);

        await using var reader = await cmd.ExecuteReaderAsync();
        (await reader.ReadAsync()).Should().BeTrue();

        var readFlag = reader.IsDBNull(0) ? (bool?)null : reader.GetBoolean(0);
        var readAt = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
        return (readFlag, readAt);
    }

    private static async Task<long> GetUnreadCountAsync(long userId)
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @userId AND (read_flag IS NULL OR read_flag = 0)";
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }
}
