namespace TicketPlatFormServer.Services.Balance;

public interface IBalanceService
{
    Task<DBModel.UserBalance> GetBalanceAsync(long userId);

    Task CreditAsync(long userId, long amount, string referenceType, long referenceId, string description);

    Task DebitAsync(long userId, long amount, string referenceType, long referenceId, string description);
}
