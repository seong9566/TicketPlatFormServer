namespace TicketPlatFormServer.Repository.Balance;

public interface IBalanceRepository
{
    Task<DBModel.UserBalance?> GetByUserIdAsync(long userId);

    Task<DBModel.UserBalance> GetOrCreateByUserIdAsync(long userId);

    Task<int> AtomicDebitAsync(long userId, long amount);

    Task<int> AtomicCreditAsync(long userId, long amount);

    Task AddTransactionAsync(DBModel.BalanceTransaction transaction);

    Task SaveChangesAsync();
}
