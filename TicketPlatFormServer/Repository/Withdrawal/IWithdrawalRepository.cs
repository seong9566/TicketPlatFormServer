namespace TicketPlatFormServer.Repository.Withdrawal;

public interface IWithdrawalRepository
{
    Task<DBModel.Withdrawal?> GetByIdAsync(long id);

    Task<DBModel.Withdrawal?> GetByIdAndUserIdAsync(long id, long userId);

    Task<DBModel.Withdrawal?> GetByIdempotencyKeyAsync(string idempotencyKey);

    Task<List<DBModel.Withdrawal>> GetByUserIdAsync(long userId, int page, int pageSize);

    Task<int> GetCountByUserIdAsync(long userId);

    Task<int> GetTodayCountByUserIdAsync(long userId);

    Task<long> GetTodaySumByUserIdAsync(long userId);

    Task<List<DBModel.Withdrawal>> GetPendingWithdrawalsAsync();

    Task<DBModel.WithdrawalStatus?> GetStatusByCodeAsync(string code);

    Task AddAsync(DBModel.Withdrawal withdrawal);

    Task UpdateAsync(DBModel.Withdrawal withdrawal);

    Task SaveChangesAsync();
}
