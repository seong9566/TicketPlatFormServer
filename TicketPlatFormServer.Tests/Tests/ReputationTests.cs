using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Reputation;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class ReputationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _buyerClient;
    private readonly HttpClient _anonClient;
    private SeededReputationData _seed = new();

    public ReputationTests(TestWebApplicationFactory factory)
    {
        _buyerClient = factory.CreateClient();
        _anonClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();

        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        var seeder = new TestDataSeeder(conn);
        var (buyerUserId, buyerEmail, _) = await seeder.CreateUserAsync();
        var (sellerUserId, sellerEmail, _) = await seeder.CreateUserAsync();

        _seed = await SeedReputationScenarioAsync(conn, buyerUserId, buyerEmail, sellerUserId, sellerEmail);
        TestAuthHelper.AddAuthHeader(_buyerClient, _seed.BuyerUserId, _seed.BuyerEmail);
    }

    public async Task DisposeAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        var deleteCommands = new[]
        {
            "DELETE FROM user_reputation WHERE transaction_id = @transactionId",
            "DELETE FROM transaction_items WHERE transaction_id = @transactionId",
            "DELETE FROM transactions WHERE id = @transactionId",
            "DELETE FROM tickets WHERE id = @ticketId",
            "DELETE FROM events WHERE id = @eventId",
            "DELETE FROM user_profile WHERE user_id IN (@buyerUserId, @sellerUserId)",
            "DELETE FROM users WHERE id IN (@buyerUserId, @sellerUserId)"
        };

        foreach (var sql in deleteCommands)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@transactionId", _seed.TransactionId);
            cmd.Parameters.AddWithValue("@ticketId", _seed.TicketId);
            cmd.Parameters.AddWithValue("@eventId", _seed.EventId);
            cmd.Parameters.AddWithValue("@buyerUserId", _seed.BuyerUserId);
            cmd.Parameters.AddWithValue("@sellerUserId", _seed.SellerUserId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task Reputation_Create_ByConfirmedTransactionBuyer_Returns201()
    {
        var request = new CreateReputationReqDto
        {
            TransactionId = _seed.TransactionId,
            Score = 5
        };

        var response = await _buyerClient.PostAsJsonAsync("/api/reputations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<long>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Reputation_Create_DuplicateReview_ReturnsConflict()
    {
        var request = new CreateReputationReqDto
        {
            TransactionId = _seed.TransactionId,
            Score = 4
        };

        var firstResponse = await _buyerClient.PostAsJsonAsync("/api/reputations", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateResponse = await _buyerClient.PostAsJsonAsync("/api/reputations", request);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await duplicateResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
        error.Should().NotBeNull();
        error!.Success.Should().BeFalse();
        error.StatusCode.Should().Be(409);
        error.Message.Should().Contain("이미");
    }

    [Fact]
    public async Task Reputation_GetByUserId_ReturnsReviewList()
    {
        var createRequest = new CreateReputationReqDto
        {
            TransactionId = _seed.TransactionId,
            Score = 5
        };

        var createResponse = await _buyerClient.PostAsJsonAsync("/api/reputations", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await _anonClient.GetAsync($"/api/users/{_seed.SellerUserId}/reputations?page=1&size=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ReputationListRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().ContainSingle();
        result.Data.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Reputation_Create_WithoutAuthentication_Returns401()
    {
        var request = new CreateReputationReqDto
        {
            TransactionId = _seed.TransactionId,
            Score = 5
        };

        var response = await _anonClient.PostAsJsonAsync("/api/reputations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task<SeededReputationData> SeedReputationScenarioAsync(
        MySqlConnection conn,
        int buyerUserId,
        string buyerEmail,
        int sellerUserId,
        string sellerEmail)
    {
        var categoryId = await GetFirstIdAsync(conn, "ticket_category");
        var ticketStatusId = await GetFirstIdAsync(conn, "ticket_statuses");
        var confirmedStatusId = await GetIdByCodeAsync(conn, "transaction_statuses", "confirmed");

        var eventId = await InsertEventAsync(conn, categoryId);
        var ticketId = await InsertTicketAsync(conn, sellerUserId, categoryId, ticketStatusId, eventId);
        var transactionId = await InsertTransactionAsync(conn, buyerUserId, sellerUserId, confirmedStatusId);
        await InsertTransactionItemAsync(conn, transactionId, ticketId);

        return new SeededReputationData
        {
            BuyerUserId = buyerUserId,
            BuyerEmail = buyerEmail,
            SellerUserId = sellerUserId,
            SellerEmail = sellerEmail,
            EventId = eventId,
            TicketId = ticketId,
            TransactionId = transactionId
        };
    }

    private static async Task<long> InsertEventAsync(MySqlConnection conn, long categoryId)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO events (
                category_id, title, description, venue_name,
                start_at, end_at, is_active, sort_order, created_at, updated_at)
            VALUES (
                @categoryId, @title, @description, @venueName,
                @startAt, @endAt, 1, 0, NOW(), NOW());", conn);
        cmd.Parameters.AddWithValue("@categoryId", categoryId);
        cmd.Parameters.AddWithValue("@title", $"테스트 공연 {Guid.NewGuid():N}");
        cmd.Parameters.AddWithValue("@description", "평판 테스트용 이벤트");
        cmd.Parameters.AddWithValue("@venueName", "테스트 공연장");
        cmd.Parameters.AddWithValue("@startAt", DateTime.UtcNow.AddDays(1));
        cmd.Parameters.AddWithValue("@endAt", DateTime.UtcNow.AddDays(1).AddHours(2));
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task<long> InsertTicketAsync(
        MySqlConnection conn,
        int sellerUserId,
        long categoryId,
        long ticketStatusId,
        long eventId)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO tickets (
                seller_id, event_id, category_id, event_datetime,
                quantity, remaining_quantity, price, status_id, created_at, updated_at)
            VALUES (
                @sellerId, @eventId, @categoryId, @eventDatetime,
                @quantity, @remainingQuantity, @price, @statusId, NOW(), NOW());", conn);
        cmd.Parameters.AddWithValue("@sellerId", sellerUserId);
        cmd.Parameters.AddWithValue("@eventId", eventId);
        cmd.Parameters.AddWithValue("@categoryId", categoryId);
        cmd.Parameters.AddWithValue("@eventDatetime", DateTime.UtcNow.AddDays(1));
        cmd.Parameters.AddWithValue("@quantity", 1);
        cmd.Parameters.AddWithValue("@remainingQuantity", 1);
        cmd.Parameters.AddWithValue("@price", 100000);
        cmd.Parameters.AddWithValue("@statusId", ticketStatusId);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task<long> InsertTransactionAsync(
        MySqlConnection conn,
        int buyerUserId,
        int sellerUserId,
        long statusId)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO transactions (
                buyer_id, seller_id, status_id, confirmed_at, amount, created_at)
            VALUES (
                @buyerId, @sellerId, @statusId, @confirmedAt, @amount, NOW());", conn);
        cmd.Parameters.AddWithValue("@buyerId", buyerUserId);
        cmd.Parameters.AddWithValue("@sellerId", sellerUserId);
        cmd.Parameters.AddWithValue("@statusId", statusId);
        cmd.Parameters.AddWithValue("@confirmedAt", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@amount", 100000);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task InsertTransactionItemAsync(MySqlConnection conn, long transactionId, long ticketId)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO transaction_items (
                transaction_id, ticket_id, quantity, unit_price, total_price, created_at)
            VALUES (
                @transactionId, @ticketId, 1, 100000, 100000, NOW());", conn);
        cmd.Parameters.AddWithValue("@transactionId", transactionId);
        cmd.Parameters.AddWithValue("@ticketId", ticketId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<long> GetFirstIdAsync(MySqlConnection conn, string tableName)
    {
        await using var cmd = new MySqlCommand($"SELECT id FROM {tableName} ORDER BY id LIMIT 1", conn);
        var result = await cmd.ExecuteScalarAsync();

        if (result == null)
        {
            throw new InvalidOperationException($"테이블 {tableName}에서 id를 찾을 수 없습니다.");
        }

        return Convert.ToInt64(result);
    }

    private static async Task<long> GetIdByCodeAsync(MySqlConnection conn, string tableName, string code)
    {
        await using var cmd = new MySqlCommand($"SELECT id FROM {tableName} WHERE LOWER(code) = LOWER(@code) LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@code", code);
        var result = await cmd.ExecuteScalarAsync();

        if (result == null)
        {
            throw new InvalidOperationException($"코드 '{code}'를 {tableName}에서 찾을 수 없습니다.");
        }

        return Convert.ToInt64(result);
    }

    private sealed class SeededReputationData
    {
        public int BuyerUserId { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public int SellerUserId { get; set; }
        public string SellerEmail { get; set; } = string.Empty;
        public long EventId { get; set; }
        public long TicketId { get; set; }
        public long TransactionId { get; set; }
    }
}
