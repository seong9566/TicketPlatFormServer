namespace TicketPlatFormServer.Repository.Balance;

public interface IBalanceRepository
{
    Task<DBModel.UserBalance?> GetByUserIdAsync(long userId);

    Task<DBModel.UserBalance> GetOrCreateByUserIdAsync(long userId);

    Task<int> AtomicDebitAsync(long userId, long amount);

    Task<int> AtomicCreditAsync(long userId, long amount);

    Task<int> AtomicPendingCreditAsync(long userId, long amount);

    Task<int> AtomicPendingDebitAsync(long userId, long amount);

    Task<List<DBModel.BalanceTransaction>> GetByUserIdAsync(long userId, int page, int pageSize);

    Task<int> GetTransactionCountByUserIdAsync(long userId);

    Task AddTransactionAsync(DBModel.BalanceTransaction transaction);

    Task SaveChangesAsync();
}
