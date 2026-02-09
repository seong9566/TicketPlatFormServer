using TicketPlatFormServer.DTO.Transaction;

namespace TicketPlatFormServer.Repository.Transactions;

public interface ITransactionHistoryRepository
{
    /// <summary>
    /// 구매 내역 조회
    /// </summary>
    /// <param name="includeTotalCount">전체 건수 조회 여부 (성능 최적화를 위해 첫 페이지에서만 true)</param>
    Task<(List<TransactionHistoryItemDto> Items, int? TotalCount)> GetPurchaseHistoryAsync(
        int userId,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt,
        int limit,
        bool includeTotalCount = false);

    /// <summary>
    /// 판매 내역 조회
    /// </summary>
    /// <param name="includeTotalCount">전체 건수 조회 여부 (성능 최적화를 위해 첫 페이지에서만 true)</param>
    Task<(List<TransactionHistoryItemDto> Items, int? TotalCount)> GetSalesHistoryAsync(
        int userId,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt,
        int limit,
        bool includeTotalCount = false);
}
