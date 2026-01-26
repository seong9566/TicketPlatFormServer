using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Transactions;

/// <summary>
/// 거래 항목(TransactionItem) 관련 Repository 인터페이스
/// </summary>
public interface ITransactionItemRepository
{
    /// <summary>
    /// 거래 항목 생성
    /// </summary>
    /// <param name="item">거래 항목 정보</param>
    /// <returns>생성된 거래 항목 (ID 포함)</returns>
    Task<TransactionItem> CreateTransactionItemAsync(TransactionItem item);
}
