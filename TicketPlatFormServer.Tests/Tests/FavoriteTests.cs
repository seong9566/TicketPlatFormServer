using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Favorite;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class FavoriteTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly HttpClient _anonClient;
    private int _favoriteUserId;
    private string _favoriteUserEmail = string.Empty;
    private int _sellerUserId;
    private long _eventId;
    private long _ticketId;

    public FavoriteTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _anonClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();

        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        var seeder = new TestDataSeeder(conn);
        (_favoriteUserId, _favoriteUserEmail, _) = await seeder.CreateUserAsync();
        (_sellerUserId, _, _) = await seeder.CreateUserAsync();

        var categoryId = await GetFirstIdAsync(conn, "ticket_category");

        _eventId = await InsertEventAsync(conn, categoryId);
        _ticketId = await InsertTicketAsync(conn, _sellerUserId, categoryId, _eventId);

        TestAuthHelper.AddAuthHeader(_client, _favoriteUserId, _favoriteUserEmail);
    }

    public async Task DisposeAsync()
    {
        await CleanupFavoriteSeedDataAsync();

        try
        {
            await TestDbManager.CleanupAsync();
        }
        catch (MySqlException)
        {
            await SafeCleanupTestUsersAsync();
        }
    }

    [Fact]
    public async Task Favorite_Toggle_Add_Returns200_AndIsFavoritedTrue()
    {
        var response = await _client.PostAsJsonAsync("/api/favorites/tickets", new FavoriteToggleReqDto
        {
            TicketId = (int)_ticketId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<FavoriteToggleRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.TicketId.Should().Be((int)_ticketId);
        result.Data.IsFavorited.Should().BeTrue();
    }

    [Fact]
    public async Task Favorite_Toggle_Remove_Returns200_AndIsFavoritedFalse()
    {
        await _client.PostAsJsonAsync("/api/favorites/tickets", new FavoriteToggleReqDto
        {
            TicketId = (int)_ticketId
        });

        var response = await _client.PostAsJsonAsync("/api/favorites/tickets", new FavoriteToggleReqDto
        {
            TicketId = (int)_ticketId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<FavoriteToggleRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.TicketId.Should().Be((int)_ticketId);
        result.Data.IsFavorited.Should().BeFalse();
    }

    [Fact]
    public async Task Favorite_GetFavoriteTickets_ReturnsUserFavorites()
    {
        await InsertFavoriteAsync(_favoriteUserId, _ticketId);

        var response = await _client.GetAsync("/api/favorites/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<FavoriteTicketListRespDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().Contain(x => x.TicketId == (int)_ticketId);
    }

    [Fact]
    public async Task Favorite_Toggle_WithoutAuth_Returns401()
    {
        var response = await _anonClient.PostAsJsonAsync("/api/favorites/tickets", new FavoriteToggleReqDto
        {
            TicketId = (int)_ticketId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task<int> GetFirstIdAsync(MySqlConnection conn, string tableName)
    {
        await using var cmd = new MySqlCommand($"SELECT id FROM {tableName} ORDER BY id LIMIT 1", conn);
        var result = await cmd.ExecuteScalarAsync();
        if (result == null)
        {
            throw new InvalidOperationException($"테이블 {tableName}에서 id를 찾을 수 없습니다.");
        }

        return Convert.ToInt32(result);
    }

    private static async Task<long> InsertEventAsync(MySqlConnection conn, int categoryId)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await using var cmd = new MySqlCommand(@"
            INSERT INTO events (
                category_id, title, description, venue_name, venue_address, start_at, end_at, is_active, sort_order, created_at, updated_at
            )
            VALUES (
                @categoryId, @title, @description, @venueName, @venueAddress, @startAt, @endAt, 1, 0, NOW(), NOW()
            );", conn);
        cmd.Parameters.AddWithValue("@categoryId", categoryId);
        cmd.Parameters.AddWithValue("@title", $"FavoriteTest Event {suffix}");
        cmd.Parameters.AddWithValue("@description", "Favorite E2E 테스트 이벤트");
        cmd.Parameters.AddWithValue("@venueName", "Favorite Test Hall");
        cmd.Parameters.AddWithValue("@venueAddress", "Seoul");
        cmd.Parameters.AddWithValue("@startAt", DateTime.UtcNow.AddDays(7));
        cmd.Parameters.AddWithValue("@endAt", DateTime.UtcNow.AddDays(7).AddHours(2));
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task<long> InsertTicketAsync(MySqlConnection conn, int sellerUserId, int categoryId, long eventId)
    {
        var tradeMethodId = await GetFirstIdAsync(conn, "trade_methods");

        await using var cmd = new MySqlCommand(@"
            INSERT INTO tickets (
                seller_id, event_id, category_id, event_datetime, quantity, is_consecutive, remaining_quantity,
                price, description, status_id, trade_method_id, has_ticket, created_at, updated_at
            )
            VALUES (
                @sellerId, @eventId, @categoryId, @eventDatetime, @quantity, 0, @remainingQuantity,
                @price, @description, 1, @tradeMethodId, 1, NOW(), NOW()
            );", conn);
        cmd.Parameters.AddWithValue("@sellerId", sellerUserId);
        cmd.Parameters.AddWithValue("@eventId", eventId);
        cmd.Parameters.AddWithValue("@categoryId", categoryId);
        cmd.Parameters.AddWithValue("@eventDatetime", DateTime.UtcNow.AddDays(7));
        cmd.Parameters.AddWithValue("@quantity", 2);
        cmd.Parameters.AddWithValue("@remainingQuantity", 2);
        cmd.Parameters.AddWithValue("@price", 100000);
        cmd.Parameters.AddWithValue("@description", "Favorite E2E 테스트 티켓");
        cmd.Parameters.AddWithValue("@tradeMethodId", tradeMethodId);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private async Task CleanupFavoriteSeedDataAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        var deleteCommands = new[]
        {
            "DELETE FROM user_favorites WHERE target_id = @ticketId AND favorite_type_id = 2",
            "DELETE FROM tickets WHERE id = @ticketId",
            "DELETE FROM events WHERE id = @eventId"
        };

        foreach (var sql in deleteCommands)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ticketId", _ticketId);
            cmd.Parameters.AddWithValue("@eventId", _eventId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
            }
        }
    }

    private static async Task InsertFavoriteAsync(int userId, long ticketId)
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(@"
            INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
            VALUES (@userId, 2, @ticketId);", conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@ticketId", ticketId);
        await cmd.ExecuteNonQueryAsync();
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
}
