using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MySqlConnector;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Dispute;
using TicketPlatFormServer.Tests.Helpers;
using Xunit;

namespace TicketPlatFormServer.Tests.Tests;

[Collection("Sequential")]
public class AdminDisputeResolutionTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _userClient;
    private readonly HttpClient _anonClient;

    private SeededDisputeData _seed = new();

    public AdminDisputeResolutionTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _adminClient = factory.CreateClient();
        _userClient = factory.CreateClient();
        _anonClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDbManager.InitializeAsync();
        _seed = await SeedDisputeScenarioAsync();

        var adminToken = TestAuthHelper.GenerateAdminToken(_seed.AdminUserId, _seed.AdminEmail);
        _adminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        TestAuthHelper.AddAuthHeader(_userClient, _seed.BuyerUserId, _seed.BuyerEmail, "user");
    }

    public async Task DisposeAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        var deleteCommands = new List<string>
        {
            "DELETE FROM disputes WHERE id = @disputeId",
            "DELETE FROM payments WHERE id = @paymentId",
            "DELETE FROM escrow WHERE id = @escrowId",
            "DELETE FROM transactions WHERE id = @transactionId",
            "DELETE FROM user_profile WHERE user_id IN (@buyerUserId, @sellerUserId, @adminUserId)",
            "DELETE FROM users WHERE id IN (@buyerUserId, @sellerUserId, @adminUserId)"
        };

        foreach (var sql in deleteCommands)
        {
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@disputeId", _seed.DisputeId);
            cmd.Parameters.AddWithValue("@paymentId", _seed.PaymentId);
            cmd.Parameters.AddWithValue("@escrowId", _seed.EscrowId);
            cmd.Parameters.AddWithValue("@transactionId", _seed.TransactionId);
            cmd.Parameters.AddWithValue("@buyerUserId", _seed.BuyerUserId);
            cmd.Parameters.AddWithValue("@sellerUserId", _seed.SellerUserId);
            cmd.Parameters.AddWithValue("@adminUserId", _seed.AdminUserId);

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
    public async Task ResolveDispute_WithAdminToken_ReturnsOk()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_BUYER",
            Reason = "구매자 승"
        };

        var response = await _adminClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminResolveDisputeRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ResolveDispute_WithUserToken_ReturnsForbidden()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_BUYER",
            Reason = "구매자 승"
        };

        var response = await _userClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResolveDispute_WithNoToken_ReturnsUnauthorized()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_BUYER",
            Reason = "구매자 승"
        };

        var response = await _anonClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResolveDispute_InvalidDisputeId_ReturnsNotFound()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_BUYER",
            Reason = "구매자 승"
        };

        var response = await _adminClient.PostAsJsonAsync("/api/admin/disputes/999999/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResolveDispute_AlreadyResolved_ReturnsConflict()
    {
        await using (var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString))
        {
            await conn.OpenAsync();
            var resolvedStatusId = await GetIdByCodeAsync(conn, "dispute_statuses", "RESOLVED_BUYER");

            await using var updateCmd = new MySqlCommand(@"
                UPDATE disputes
                SET status_id = @statusId, resolved_at = NOW(), resolution_note = '이미 처리된 분쟁'
                WHERE id = @disputeId", conn);
            updateCmd.Parameters.AddWithValue("@statusId", resolvedStatusId);
            updateCmd.Parameters.AddWithValue("@disputeId", _seed.DisputeId);
            await updateCmd.ExecuteNonQueryAsync();
        }

        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_SELLER",
            Reason = "판매자 승"
        };

        var response = await _adminClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ResolveDispute_AsBuyerWins_EscrowRefunded()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_BUYER",
            Reason = "구매자 승"
        };

        var response = await _adminClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var escrowStatusCode = await GetEscrowStatusCodeAsync(_seed.TransactionId);
        escrowStatusCode.Should().Be("refunded");
    }

    [Fact]
    public async Task ResolveDispute_AsSellerWins_EscrowReleased()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "RESOLVED_SELLER",
            Reason = "판매자 승"
        };

        var response = await _adminClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var escrowStatusCode = await GetEscrowStatusCodeAsync(_seed.TransactionId);
        escrowStatusCode.Should().Be("released");
    }

    [Fact]
    public async Task ResolveDispute_AsRejected_EscrowUnfrozen()
    {
        var request = new AdminResolveDisputeReqDto
        {
            ResolutionCode = "REJECTED",
            Reason = "신고 근거 부족"
        };

        var response = await _adminClient.PostAsJsonAsync($"/api/admin/disputes/{_seed.DisputeId}/resolve", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var escrowStatusCode = await GetEscrowStatusCodeAsync(_seed.TransactionId);
        escrowStatusCode.Should().Be("holding");
    }

    [Fact]
    public async Task GetAllDisputes_WithAdminToken_ReturnsList()
    {
        var response = await _adminClient.GetAsync("/api/admin/disputes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminDisputeListRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDisputeDetail_WithAdminToken_ReturnsDetail()
    {
        var response = await _adminClient.GetAsync($"/api/admin/disputes/{_seed.DisputeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DisputeDetailRespDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(_seed.DisputeId);
    }

    private static async Task<SeededDisputeData> SeedDisputeScenarioAsync()
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        var userRoleId = await GetIdByCodeOrFirstAsync(conn, "auth_roles", "user");
        var adminRoleId = await GetIdByCodeOrFirstAsync(conn, "auth_roles", "admin");
        var providerId = await GetIdByCodeOrFirstAsync(conn, "auth_providers", "local");

        var transactionPaidStatusId = await GetIdByCodeAsync(conn, "transaction_statuses", "paid");
        var escrowFrozenStatusId = await GetIdByCodeAsync(conn, "escrow_statuses", "frozen");
        var disputePendingStatusId = await GetIdByCodeAsync(conn, "dispute_statuses", "PENDING");

        var disputeTypeId = await GetFirstIdAsync(conn, "dispute_types");
        var paymentMethodId = await GetFirstIdAsync(conn, "payment_methods");
        var paymentStatusId = await GetFirstIdAsync(conn, "payment_statuses");

        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var buyerEmail = $"test_buyer_{uniqueSuffix}@test.com";
        var sellerEmail = $"test_seller_{uniqueSuffix}@test.com";
        var adminEmail = $"test_admin_{uniqueSuffix}@test.com";

        var buyerUserId = await InsertUserAsync(conn, buyerEmail, $"Buyer_{uniqueSuffix}", providerId, userRoleId);
        var sellerUserId = await InsertUserAsync(conn, sellerEmail, $"Seller_{uniqueSuffix}", providerId, userRoleId);
        var adminUserId = await InsertUserAsync(conn, adminEmail, $"Admin_{uniqueSuffix}", providerId, adminRoleId);

        var transactionId = await InsertTransactionAsync(conn, buyerUserId, sellerUserId, transactionPaidStatusId, 150000);
        var escrowId = await InsertEscrowAsync(conn, transactionId, escrowFrozenStatusId, 150000, 7500, 142500);

        var paymentId = await InsertPaymentAsync(
            conn,
            transactionId,
            paymentMethodId,
            paymentStatusId,
            "test_payment_key_dummy",
            $"test-order-{uniqueSuffix}",
            150000);

        var disputeId = await InsertDisputeAsync(
            conn,
            transactionId,
            buyerUserId,
            disputeTypeId,
            disputePendingStatusId,
            "관리자 분쟁 해결 RED 테스트 시딩 데이터");

        return new SeededDisputeData
        {
            BuyerUserId = (int)buyerUserId,
            BuyerEmail = buyerEmail,
            SellerUserId = (int)sellerUserId,
            SellerEmail = sellerEmail,
            AdminUserId = (int)adminUserId,
            AdminEmail = adminEmail,
            TransactionId = transactionId,
            EscrowId = escrowId,
            PaymentId = paymentId,
            DisputeId = disputeId
        };
    }

    private static async Task<long> InsertUserAsync(
        MySqlConnection conn,
        string email,
        string nickname,
        long providerId,
        long roleId)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test1234!@#");
        var phone = $"010{Random.Shared.Next(10000000, 99999999)}";

        await using var userCmd = new MySqlCommand(@"
            INSERT INTO users (email, password_hash, phone, provider_id, role_id, created_at, is_deleted)
            VALUES (@email, @passwordHash, @phone, @providerId, @roleId, NOW(), 0);", conn);
        userCmd.Parameters.AddWithValue("@email", email);
        userCmd.Parameters.AddWithValue("@passwordHash", hashedPassword);
        userCmd.Parameters.AddWithValue("@phone", phone);
        userCmd.Parameters.AddWithValue("@providerId", providerId);
        userCmd.Parameters.AddWithValue("@roleId", roleId);
        await userCmd.ExecuteNonQueryAsync();

        var userId = userCmd.LastInsertedId;

        await using var profileCmd = new MySqlCommand(@"
            INSERT INTO user_profile (user_id, nickname)
            VALUES (@userId, @nickname);", conn);
        profileCmd.Parameters.AddWithValue("@userId", userId);
        profileCmd.Parameters.AddWithValue("@nickname", nickname);
        await profileCmd.ExecuteNonQueryAsync();

        return userId;
    }

    private static async Task<long> InsertTransactionAsync(
        MySqlConnection conn,
        long buyerId,
        long sellerId,
        long statusId,
        int amount)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO transactions (buyer_id, seller_id, status_id, amount, created_at)
            VALUES (@buyerId, @sellerId, @statusId, @amount, NOW());", conn);
        cmd.Parameters.AddWithValue("@buyerId", buyerId);
        cmd.Parameters.AddWithValue("@sellerId", sellerId);
        cmd.Parameters.AddWithValue("@statusId", statusId);
        cmd.Parameters.AddWithValue("@amount", amount);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task<long> InsertEscrowAsync(
        MySqlConnection conn,
        long transactionId,
        long statusId,
        int amount,
        int feeAmount,
        int sellerAmount)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO escrow (transaction_id, amount, fee_amount, seller_amount, status_id, created_at, updated_at)
            VALUES (@transactionId, @amount, @feeAmount, @sellerAmount, @statusId, NOW(), NOW());", conn);
        cmd.Parameters.AddWithValue("@transactionId", transactionId);
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.Parameters.AddWithValue("@feeAmount", feeAmount);
        cmd.Parameters.AddWithValue("@sellerAmount", sellerAmount);
        cmd.Parameters.AddWithValue("@statusId", statusId);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task<long> InsertPaymentAsync(
        MySqlConnection conn,
        long transactionId,
        long methodId,
        long statusId,
        string paymentKey,
        string orderId,
        ulong amount)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO payments (
                transaction_id, pg_provider, payment_key, order_id, amount,
                method_id, paid_at, status_id, use_escrow, is_partial_cancelable, payment_type, country)
            VALUES (
                @transactionId, 'toss', @paymentKey, @orderId, @amount,
                @methodId, NOW(), @statusId, 1, 0, 'NORMAL', 'KR');", conn);
        cmd.Parameters.AddWithValue("@transactionId", transactionId);
        cmd.Parameters.AddWithValue("@paymentKey", paymentKey);
        cmd.Parameters.AddWithValue("@orderId", orderId);
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.Parameters.AddWithValue("@methodId", methodId);
        cmd.Parameters.AddWithValue("@statusId", statusId);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
    }

    private static async Task<long> InsertDisputeAsync(
        MySqlConnection conn,
        long transactionId,
        long claimantId,
        long typeId,
        long statusId,
        string description)
    {
        await using var cmd = new MySqlCommand(@"
            INSERT INTO disputes (transaction_id, claimant_id, type_id, description, status_id, created_at)
            VALUES (@transactionId, @claimantId, @typeId, @description, @statusId, NOW());", conn);
        cmd.Parameters.AddWithValue("@transactionId", transactionId);
        cmd.Parameters.AddWithValue("@claimantId", claimantId);
        cmd.Parameters.AddWithValue("@typeId", typeId);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@statusId", statusId);
        await cmd.ExecuteNonQueryAsync();

        return cmd.LastInsertedId;
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

    private static async Task<long> GetIdByCodeOrFirstAsync(MySqlConnection conn, string tableName, string code)
    {
        await using var cmd = new MySqlCommand($"SELECT id FROM {tableName} WHERE LOWER(code) = LOWER(@code) LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@code", code);
        var result = await cmd.ExecuteScalarAsync();

        if (result != null)
        {
            return Convert.ToInt64(result);
        }

        return await GetFirstIdAsync(conn, tableName);
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

    private static async Task<string?> GetEscrowStatusCodeAsync(long transactionId)
    {
        await using var conn = new MySqlConnection(TestWebApplicationFactory.TestConnectionString);
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(@"
            SELECT es.code
            FROM escrow e
            INNER JOIN escrow_statuses es ON es.id = e.status_id
            WHERE e.transaction_id = @transactionId
            LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@transactionId", transactionId);

        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    private sealed class SeededDisputeData
    {
        public int BuyerUserId { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public int SellerUserId { get; set; }
        public string SellerEmail { get; set; } = string.Empty;
        public int AdminUserId { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public long TransactionId { get; set; }
        public long EscrowId { get; set; }
        public long PaymentId { get; set; }
        public long DisputeId { get; set; }
    }
}
