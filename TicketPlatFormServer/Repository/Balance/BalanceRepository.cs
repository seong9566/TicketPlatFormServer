using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace TicketPlatFormServer.Repository.Balance;

public class BalanceRepository(TicketContext context) : IBalanceRepository
{
    public async Task<DBModel.UserBalance?> GetByUserIdAsync(int userId)
    {
        return await context.UserBalances.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<DBModel.UserBalance> GetOrCreateByUserIdAsync(int userId)
    {
        var balance = await GetByUserIdAsync(userId);
        if (balance != null)
        {
            return balance;
        }

        balance = new DBModel.UserBalance
        {
            UserId = userId,
            Available = 0,
            Pending = 0,
            TotalEarned = 0,
            TotalWithdrawn = 0
        };

        await context.UserBalances.AddAsync(balance);
        await context.SaveChangesAsync();

        return balance;
    }

    public async Task<int> AtomicDebitAsync(int userId, long amount)
    {
        const string sql = @"
UPDATE user_balance
SET available = available - @amount
WHERE user_id = @userId
  AND available >= @amount";

        return await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@amount", amount),
            new MySqlParameter("@userId", userId));
    }

    public async Task<int> AtomicCreditAsync(int userId, long amount)
    {
        const string sql = @"
UPDATE user_balance
SET available = available + @amount
WHERE user_id = @userId";

        return await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@amount", amount),
            new MySqlParameter("@userId", userId));
    }

    public async Task<int> AtomicPendingCreditAsync(int userId, long amount)
    {
        const string sql = @"
UPDATE user_balance
SET pending = pending + @amount
WHERE user_id = @userId";

        return await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@amount", amount),
            new MySqlParameter("@userId", userId));
    }

    public async Task<int> AtomicPendingDebitAsync(int userId, long amount)
    {
        const string sql = @"
UPDATE user_balance
SET pending = pending - @amount
WHERE user_id = @userId
  AND pending >= @amount";

        return await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@amount", amount),
            new MySqlParameter("@userId", userId));
    }

    public async Task<List<DBModel.BalanceTransaction>> GetByUserIdAsync(int userId, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;

        return await context.BalanceTransactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTransactionCountByUserIdAsync(int userId)
    {
        return await context.BalanceTransactions.CountAsync(x => x.UserId == userId);
    }

    public async Task AddTransactionAsync(DBModel.BalanceTransaction transaction)
    {
        await context.BalanceTransactions.AddAsync(transaction);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
