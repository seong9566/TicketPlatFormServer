using TicketPlatFormServer.DTO.Transaction;

namespace TicketPlatFormServer.Services.Transaction;

public interface ITransactionService
{
    Task<TransactionHistoryRespDto> GetPurchaseHistoryAsync(
        int userId,
        string? status,
        string? period,
        string? sortBy,
        string? cursor,
        int? limit);

    Task<TransactionHistoryRespDto> GetSalesHistoryAsync(
        int userId,
        string? status,
        string? period,
        string? sortBy,
        string? cursor,
        int? limit);
}
