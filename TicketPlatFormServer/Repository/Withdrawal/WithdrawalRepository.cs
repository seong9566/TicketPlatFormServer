using Microsoft.EntityFrameworkCore;

namespace TicketPlatFormServer.Repository.Withdrawal;

public class WithdrawalRepository(TicketContext context) : IWithdrawalRepository
{
    public async Task<DBModel.Withdrawal?> GetByIdAsync(long id)
    {
        return await context.Withdrawals
            .Where(x => x.Id == id)
            .Include(x => x.Status)
            .Include(x => x.BankAccount)
            .FirstOrDefaultAsync();
    }

    public async Task<DBModel.Withdrawal?> GetByIdAndUserIdAsync(long id, long userId)
    {
        return await context.Withdrawals
            .Where(x => x.Id == id && x.UserId == userId)
            .Include(x => x.Status)
            .Include(x => x.BankAccount)
            .FirstOrDefaultAsync();
    }

    public async Task<DBModel.Withdrawal?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await context.Withdrawals
            .Where(x => x.IdempotencyKey == idempotencyKey)
            .Include(x => x.Status)
            .Include(x => x.BankAccount)
            .FirstOrDefaultAsync();
    }

    public async Task<List<DBModel.Withdrawal>> GetByUserIdAsync(long userId, int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;

        return await context.Withdrawals
            .Where(x => x.UserId == userId)
            .Include(x => x.Status)
            .Include(x => x.BankAccount)
            .OrderByDescending(x => x.RequestedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(long userId)
    {
        return await context.Withdrawals.CountAsync(x => x.UserId == userId);
    }

    public async Task<int> GetTodayCountByUserIdAsync(long userId)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        return await context.Withdrawals.CountAsync(x => x.UserId == userId
            && x.RequestedAt >= todayStart
            && x.RequestedAt < tomorrowStart);
    }

    public async Task<long> GetTodaySumByUserIdAsync(long userId)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        return await context.Withdrawals
            .Where(x => x.UserId == userId
                && x.RequestedAt >= todayStart
                && x.RequestedAt < tomorrowStart)
            .SumAsync(x => (long?)x.Amount) ?? 0;
    }

    public async Task<List<DBModel.Withdrawal>> GetPendingWithdrawalsAsync()
    {
        return await context.Withdrawals
            .Where(x => x.Status.Code == "requested")
            .Include(x => x.Status)
            .Include(x => x.BankAccount)
            .Include(x => x.User)
            .OrderBy(x => x.RequestedAt)
            .ToListAsync();
    }

    public async Task<DBModel.WithdrawalStatus?> GetStatusByCodeAsync(string code)
    {
        return await context.WithdrawalStatuses.FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task AddAsync(DBModel.Withdrawal withdrawal)
    {
        await context.Withdrawals.AddAsync(withdrawal);
    }

    public async Task UpdateAsync(DBModel.Withdrawal withdrawal)
    {
        context.Entry(withdrawal).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
