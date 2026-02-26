using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace TicketPlatFormServer.Repository.Balance;

public class BalanceRepository(TicketContext context) : IBalanceRepository
{
    public async Task<DBModel.UserBalance?> GetByUserIdAsync(long userId)
    {
        return await context.UserBalances.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<DBModel.UserBalance> GetOrCreateByUserIdAsync(long userId)
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

    public async Task<int> AtomicDebitAsync(long userId, long amount)
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

    public async Task<int> AtomicCreditAsync(long userId, long amount)
    {
        const string sql = @"
UPDATE user_balance
SET available = available + @amount
WHERE user_id = @userId";

        return await context.Database.ExecuteSqlRawAsync(sql,
            new MySqlParameter("@amount", amount),
            new MySqlParameter("@userId", userId));
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
