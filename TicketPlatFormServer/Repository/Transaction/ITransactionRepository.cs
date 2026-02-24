using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Transactions;

/// <summary>
/// 거래(Transaction) 관련 Repository 인터페이스
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// ID로 거래 조회
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <returns>거래 정보 (없으면 null)</returns>
    Task<DBModel.Transaction?> GetTransactionById(long transactionId);

    /// <summary>
    /// 거래 소유권 검증 (BuyerId, SellerId 일치 여부)
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <param name="buyerId">구매자 ID</param>
    /// <param name="sellerId">판매자 ID</param>
    /// <returns>소유권 일치 여부</returns>
    Task<bool> ValidateTransactionOwnership(long transactionId, long buyerId, long sellerId);

    /// <summary>
    /// 거래 상태 업데이트
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <param name="statusId">상태 ID</param>
    Task UpdateTransactionStatusAsync(long transactionId, long statusId);

    /// <summary>
    /// 거래 취소 시각 업데이트
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <param name="cancelledAt">취소 시각</param>
    Task UpdateTransactionCancelledAtAsync(long transactionId, DateTime cancelledAt);

    /// <summary>
    /// 상세 정보와 함께 거래 조회 (Buyer, Seller, TransactionItems with Ticket & Event)
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <returns>상세 거래 정보 (없으면 null)</returns>
    Task<DBModel.Transaction?> GetTransactionWithDetailsAsync(long transactionId);

    /// <summary>
    /// 예약 만료된 거래 목록 조회 (pending_payment)
    /// </summary>
    /// <param name="utcNow">현재 시각 (UTC)</param>
    /// <returns>만료된 거래 목록</returns>
    Task<List<DBModel.Transaction>> GetExpiredPendingTransactionsAsync(DateTime utcNow);

    Task<List<DBModel.Transaction>> GetAutoConfirmDueTransactionsAsync(DateTime utcNow);

    /// <summary>
    /// 거래 생성
    /// </summary>
    /// <param name="transaction">거래 정보</param>
    /// <returns>생성된 거래 (ID 포함)</returns>
    Task<DBModel.Transaction> CreateTransactionAsync(DBModel.Transaction transaction);

    /// <summary>
    /// Code로 TransactionStatus 조회
    /// </summary>
    /// <param name="code">상태 코드 (예: "pending", "paid", "confirmed")</param>
    /// <returns>TransactionStatus (없으면 null)</returns>
    Task<DBModel.TransactionStatus?> GetTransactionStatusByCodeAsync(string code);

    Task<PaymentPreviewReadModel?> GetPaymentPreviewAsync(long transactionId, int buyerId);
}
