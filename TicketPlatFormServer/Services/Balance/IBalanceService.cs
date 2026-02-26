namespace TicketPlatFormServer.Services.Balance;

public interface IBalanceService
{
    Task<DBModel.UserBalance> GetBalanceAsync(int userId);

    Task<DTO.Balance.BalanceResponseDto> AdminAdjustBalanceAsync(int userId, long amount, string reason);

    Task CreditAsync(int userId, long amount, string referenceType, long referenceId, string description);

    Task DebitAsync(int userId, long amount, string referenceType, long referenceId, string description);
}
