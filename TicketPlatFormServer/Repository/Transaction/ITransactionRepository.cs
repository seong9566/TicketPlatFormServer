using TicketPlatFormServer.DBModel;

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
}
