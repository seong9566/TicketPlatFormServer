using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Transactions;

/// <summary>
/// 거래(Transaction) 관련 Repository 구현체
/// Primary Constructor 패턴 사용
/// </summary>
public class TransactionRepository(TicketContext context, IDbConnection dapper) : ITransactionRepository
{
    /// <summary>
    /// ID로 거래 조회
    /// </summary>
    public async Task<DBModel.Transaction?> GetTransactionById(long transactionId)
    {
        return await context.Transactions
            .Where(t => t.Id == transactionId && t.DeletedAt == null)
            .Include(t => t.Status)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 거래 소유권 검증 (BuyerId, SellerId 일치 여부)
    /// </summary>
    public async Task<bool> ValidateTransactionOwnership(long transactionId, long buyerId, long sellerId)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM transactions
            WHERE id = @TransactionId
              AND buyer_id = @BuyerId
              AND seller_id = @SellerId
              AND deleted_at IS NULL
        ";

        var count = await dapper.ExecuteScalarAsync<int>(query, new
        {
            TransactionId = transactionId,
            BuyerId = buyerId,
            SellerId = sellerId
        });

        return count > 0;
    }

    /// <summary>
    /// 거래 상태 업데이트
    /// </summary>
    public async Task UpdateTransactionStatusAsync(long transactionId, long statusId)
    {
        const string sql = @"
            UPDATE transactions
            SET status_id = @StatusId
            WHERE id = @TransactionId
              AND deleted_at IS NULL
        ";

        await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@TransactionId", transactionId),
            new MySqlParameter("@StatusId", statusId));
    }

    /// <summary>
    /// 거래 취소 시각 업데이트
    /// </summary>
    public async Task UpdateTransactionCancelledAtAsync(long transactionId, DateTime cancelledAt)
    {
        const string sql = @"
            UPDATE transactions
            SET cancelled_at = @CancelledAt
            WHERE id = @TransactionId
              AND deleted_at IS NULL
        ";

        await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@TransactionId", transactionId),
            new MySqlParameter("@CancelledAt", cancelledAt));
    }

    /// <summary>
    /// 상세 정보와 함께 거래 조회 (Buyer, Seller, TransactionItems)
    /// </summary>
    public async Task<DBModel.Transaction?> GetTransactionWithDetailsAsync(long transactionId)
    {
        return await context.Transactions
            .Where(t => t.Id == transactionId && t.DeletedAt == null)
            .Include(t => t.Status)
            .Include(t => t.TransactionItems)
            .AsSplitQuery()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 예약 만료된 거래 목록 조회 (pending_payment)
    /// </summary>
    public async Task<List<DBModel.Transaction>> GetExpiredPendingTransactionsAsync(DateTime utcNow)
    {
        return await context.Transactions
            .Where(t => t.DeletedAt == null
                        && t.CancelledAt == null
                        && t.ReservationExpiresAt != null
                        && t.ReservationExpiresAt < utcNow
                        && t.Status.Code == "pending_payment")
            .Include(t => t.Status)
            .Include(t => t.TransactionItems)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<DBModel.Transaction>> GetAutoConfirmDueTransactionsAsync(DateTime utcNow)
    {
        return await context.Transactions
            .Where(t => t.DeletedAt == null
                        && t.CancelledAt == null
                        && t.ConfirmedAt == null
                        && t.AutoConfirmAt != null
                        && t.AutoConfirmAt <= utcNow
                        && t.Status.Code == "paid")
            .Include(t => t.Status)
            .AsSplitQuery()
            .ToListAsync();
    }

    /// <summary>
    /// 거래 생성
    /// </summary>
    public async Task<DBModel.Transaction> CreateTransactionAsync(DBModel.Transaction transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    /// <summary>
    /// Code로 TransactionStatus 조회 (캐싱 권장)
    /// </summary>
    public async Task<DBModel.TransactionStatus?> GetTransactionStatusByCodeAsync(string code)
    {
        return await context.TransactionStatuses
            .Where(ts => ts.Code == code && ts.IsActive == true)
            .FirstOrDefaultAsync();
    }

    public async Task<PaymentPreviewReadModel?> GetPaymentPreviewAsync(long transactionId, long buyerId)
    {
        const string query = @"
            SELECT
                CAST(MIN(tick.id) AS SIGNED) AS TicketId,
                MIN(e.poster_image_url) AS ThumbnailUrl,
                MIN(CONCAT_WS(' ',
                    NULLIF(sl.location_name, ''),
                    NULLIF(a.area_name, ''),
                    NULLIF(sg.name_ko, ''),
                    NULLIF(tick.row, '')
                )) AS SeatInfo,
                CAST(SUM(ti.quantity) AS SIGNED) AS Quantity,
                CAST(MIN(ti.unit_price) AS SIGNED) AS UnitPrice,
                CAST(SUM(ti.total_price) AS SIGNED) AS TotalAmount,
                CAST(MIN(e.id) AS SIGNED) AS EventId,
                MIN(e.title) AS EventTitle,
                MIN(tick.event_datetime) AS EventDateTime,
                MIN(e.venue_name) AS VenueName
            FROM transactions t
            INNER JOIN transaction_items ti ON ti.transaction_id = t.id
            INNER JOIN tickets tick ON tick.id = ti.ticket_id
            LEFT JOIN events e ON e.id = tick.event_id
            LEFT JOIN event_seat_locations sl ON sl.id = tick.seat_location_id
            LEFT JOIN event_seat_areas a ON a.id = tick.area_id
            LEFT JOIN event_seat_grades sg ON sg.id = tick.seat_grade_id
            WHERE t.id = @TransactionId
              AND t.buyer_id = @BuyerId
              AND t.deleted_at IS NULL
            GROUP BY t.id
        ";

        return await dapper.QuerySingleOrDefaultAsync<PaymentPreviewReadModel>(query, new
        {
            TransactionId = transactionId,
            BuyerId = buyerId
        });
    }
}
