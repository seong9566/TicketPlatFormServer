namespace TicketPlatFormServer.Repository.Balance;

public interface IBalanceRepository
{
    Task<DBModel.UserBalance?> GetByUserIdAsync(int userId);

    Task<DBModel.UserBalance> GetOrCreateByUserIdAsync(int userId);

    Task<int> AtomicDebitAsync(int userId, long amount);

    Task<int> AtomicCreditAsync(int userId, long amount);

    Task<int> AtomicPendingCreditAsync(int userId, long amount);

    Task<int> AtomicPendingDebitAsync(int userId, long amount);

    Task<List<DBModel.BalanceTransaction>> GetByUserIdAsync(int userId, int page, int pageSize);

    Task<int> GetTransactionCountByUserIdAsync(int userId);

    Task AddTransactionAsync(DBModel.BalanceTransaction transaction);

    Task SaveChangesAsync();
}
