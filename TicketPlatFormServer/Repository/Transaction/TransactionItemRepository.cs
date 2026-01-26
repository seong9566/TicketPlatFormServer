using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Transactions;

/// <summary>
/// 거래 항목(TransactionItem) 관련 Repository 구현체
/// Primary Constructor 패턴 사용
/// </summary>
public class TransactionItemRepository(TicketContext context) : ITransactionItemRepository
{
    /// <summary>
    /// 거래 항목 생성
    /// </summary>
    public async Task<TransactionItem> CreateTransactionItemAsync(TransactionItem item)
    {
        item.CreatedAt = DateTime.UtcNow;
        context.TransactionItems.Add(item);
        await context.SaveChangesAsync();
        return item;
    }
}
