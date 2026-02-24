using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Payment;

/// <summary>
/// 결제(Payment) 및 에스크로(Escrow) 관련 Repository 구현체
/// Primary Constructor 패턴 사용
/// </summary>
public class PaymentRepository(
    TicketContext context,
    IDbConnection dapper,
    IMemoryCache cache) : IPaymentRepository
{
    private const string CacheKeyPrefix = "PaymentRepo_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    // ==================== Payment CRUD ====================

    /// <summary>
    /// 결제 정보 생성
    /// </summary>
    public async Task<DBModel.Payment> CreatePaymentAsync(DBModel.Payment payment)
    {
        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    /// <summary>
    /// OrderId로 결제 조회
    /// </summary>
    public async Task<DBModel.Payment?> GetPaymentByOrderIdAsync(string orderId)
    {
        return await context.Payments
            .Where(p => p.OrderId == orderId)
            .Include(p => p.Status)
            .Include(p => p.Method)
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// PaymentKey로 결제 조회
    /// </summary>
    public async Task<DBModel.Payment?> GetPaymentByPaymentKeyAsync(string paymentKey)
    {
        return await context.Payments
            .Where(p => p.PaymentKey == paymentKey)
            .Include(p => p.Status)
            .Include(p => p.Method)
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// TransactionId로 결제 조회
    /// </summary>
    public async Task<DBModel.Payment?> GetPaymentByTransactionIdAsync(long transactionId)
    {
        return await context.Payments
            .Where(p => p.TransactionId == transactionId)
            .Include(p => p.Status)
            .Include(p => p.Method)
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 결제 상태 업데이트
    /// </summary>
    public async Task UpdatePaymentStatusAsync(long paymentId, long statusId, DateTime? paidAt = null)
    {
        const string query = @"
            UPDATE payments
            SET status_id = @StatusId,
                paid_at = COALESCE(@PaidAt, paid_at)
            WHERE id = @PaymentId
        ";

        await dapper.ExecuteAsync(query, new
        {
            PaymentId = paymentId,
            StatusId = statusId,
            PaidAt = paidAt
        });
    }

    // ==================== Escrow 관리 ====================

    /// <summary>
    /// 에스크로 생성
    /// </summary>
    public async Task<Escrow> CreateEscrowAsync(Escrow escrow)
    {
        escrow.CreatedAt = DateTime.UtcNow;
        escrow.UpdatedAt = DateTime.UtcNow;

        context.Escrows.Add(escrow);
        await context.SaveChangesAsync();
        return escrow;
    }

    /// <summary>
    /// TransactionId로 에스크로 조회
    /// </summary>
    public async Task<Escrow?> GetEscrowByTransactionIdAsync(long transactionId)
    {
        return await context.Escrows
            .Where(e => e.TransactionId == transactionId)
            .Include(e => e.Status)
            .Include(e => e.Transaction)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 에스크로 해제 (정산)
    /// </summary>
    public async Task<int> ReleaseEscrowAsync(long escrowId, long statusId, long holdingStatusId, DateTime releasedAt)
    {
        const string query = @"
            UPDATE escrow
            SET status_id = @StatusId,
                released_at = @ReleasedAt,
                updated_at = @UpdatedAt
            WHERE id = @EscrowId
              AND released_at IS NULL
              AND refunded_at IS NULL
              AND status_id = @HoldingStatusId
        ";

        return await dapper.ExecuteAsync(query, new
        {
            EscrowId = escrowId,
            StatusId = statusId,
            HoldingStatusId = holdingStatusId,
            ReleasedAt = releasedAt,
            UpdatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 에스크로 환불
    /// </summary>
    public async Task RefundEscrowAsync(long escrowId, long statusId, DateTime refundedAt)
    {
        const string query = @"
            UPDATE escrow
            SET status_id = @StatusId,
                refunded_at = @RefundedAt,
                updated_at = @UpdatedAt
            WHERE id = @EscrowId
        ";

        await dapper.ExecuteAsync(query, new
        {
            EscrowId = escrowId,
            StatusId = statusId,
            RefundedAt = refundedAt,
            UpdatedAt = DateTime.UtcNow
        });
    }

    // ==================== 상태 코드 매핑 (캐싱) ====================

    /// <summary>
    /// Code로 PaymentMethod 조회 (캐싱)
    /// </summary>
    public async Task<PaymentMethod?> GetPaymentMethodByCodeAsync(string code)
    {
        var cacheKey = $"{CacheKeyPrefix}PaymentMethod_{code}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;

            return await context.PaymentMethods
                .Where(pm => pm.Code == code && pm.IsActive == true)
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Code로 PaymentStatus 조회 (캐싱)
    /// </summary>
    public async Task<PaymentStatus?> GetPaymentStatusByCodeAsync(string code)
    {
        var cacheKey = $"{CacheKeyPrefix}PaymentStatus_{code}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;

            return await context.PaymentStatuses
                .Where(ps => ps.Code == code && ps.IsActive == true)
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Code로 TransactionStatus 조회 (캐싱)
    /// </summary>
    public async Task<TransactionStatus?> GetTransactionStatusByCodeAsync(string code)
    {
        var cacheKey = $"{CacheKeyPrefix}TransactionStatus_{code}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;

            return await context.TransactionStatuses
                .Where(ts => ts.Code == code && ts.IsActive == true)
                .FirstOrDefaultAsync();
        });
    }

    /// <summary>
    /// Code로 EscrowStatus 조회 (캐싱)
    /// </summary>
    public async Task<EscrowStatus?> GetEscrowStatusByCodeAsync(string code)
    {
        var cacheKey = $"{CacheKeyPrefix}EscrowStatus_{code}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;

            return await context.EscrowStatuses
                .Where(es => es.Code == code && es.IsActive == true)
                .FirstOrDefaultAsync();
        });
    }

    public async Task<SettlementStatus?> GetSettlementStatusByCodeAsync(string code)
    {
        var cacheKey = $"{CacheKeyPrefix}SettlementStatus_{code}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;

            return await context.SettlementStatuses
                .Where(ss => ss.Code == code && ss.IsActive == true)
                .FirstOrDefaultAsync();
        });
    }

    // ==================== 결제 수단별 상세 정보 ====================

    /// <summary>
    /// 카드 결제 상세 정보 생성
    /// </summary>
    public async Task<PaymentCardDetail> CreateCardDetailAsync(PaymentCardDetail cardDetail)
    {
        context.Set<PaymentCardDetail>().Add(cardDetail);
        await context.SaveChangesAsync();
        return cardDetail;
    }

    /// <summary>
    /// 가상계좌 결제 상세 정보 생성
    /// </summary>
    public async Task<PaymentVirtualAccountDetail> CreateVirtualAccountDetailAsync(PaymentVirtualAccountDetail vaDetail)
    {
        context.Set<PaymentVirtualAccountDetail>().Add(vaDetail);
        await context.SaveChangesAsync();
        return vaDetail;
    }

    /// <summary>
    /// 간편결제 상세 정보 생성
    /// </summary>
    public async Task<PaymentEasyPayDetail> CreateEasyPayDetailAsync(PaymentEasyPayDetail easyPayDetail)
    {
        context.Set<PaymentEasyPayDetail>().Add(easyPayDetail);
        await context.SaveChangesAsync();
        return easyPayDetail;
    }

    /// <summary>
    /// 현금영수증 생성
    /// </summary>
    public async Task<PaymentCashReceipt> CreateCashReceiptAsync(PaymentCashReceipt cashReceipt)
    {
        context.Set<PaymentCashReceipt>().Add(cashReceipt);
        await context.SaveChangesAsync();
        return cashReceipt;
    }

    /// <summary>
    /// 결제 거래 이벤트 생성
    /// </summary>
    public async Task<PaymentTransaction> CreateTransactionAsync(PaymentTransaction transaction)
    {
        context.Set<PaymentTransaction>().Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    /// <summary>
    /// PaymentId로 카드 상세 정보 조회
    /// </summary>
    public async Task<PaymentCardDetail?> GetCardDetailByPaymentIdAsync(long paymentId)
    {
        return await context.Set<PaymentCardDetail>()
            .Where(c => c.PaymentId == (ulong)paymentId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// PaymentId로 가상계좌 상세 정보 조회
    /// </summary>
    public async Task<PaymentVirtualAccountDetail?> GetVirtualAccountDetailByPaymentIdAsync(long paymentId)
    {
        return await context.Set<PaymentVirtualAccountDetail>()
            .Where(v => v.PaymentId == (ulong)paymentId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// PaymentId로 간편결제 상세 정보 조회
    /// </summary>
    public async Task<PaymentEasyPayDetail?> GetEasyPayDetailByPaymentIdAsync(long paymentId)
    {
        return await context.Set<PaymentEasyPayDetail>()
            .Where(e => e.PaymentId == (ulong)paymentId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// PaymentId로 거래 이벤트 목록 조회
    /// </summary>
    public async Task<List<PaymentTransaction>> GetTransactionsByPaymentIdAsync(long paymentId)
    {
        return await context.Set<PaymentTransaction>()
            .Where(t => t.PaymentId == (ulong)paymentId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }
}
