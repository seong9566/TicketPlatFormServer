using TicketPlatFormServer.DTO.Transaction;

namespace TicketPlatFormServer.Repository.Transactions;

public interface ITransactionHistoryRepository
{
    Task<(List<TransactionHistoryItemDto> Items, int TotalCount)> GetPurchaseHistoryAsync(
        int userId,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt,
        int limit);

    Task<(List<TransactionHistoryItemDto> Items, int TotalCount)> GetSalesHistoryAsync(
        int userId,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt,
        int limit);
}
