using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace TicketPlatFormServer.Repository.Withdrawal;

public class WithdrawalRepository(TicketContext context, ILogger<WithdrawalRepository> logger) : IWithdrawalRepository
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
        try
        {
            return await context.Withdrawals
                .Where(x => x.Status.Code == "requested")
                .Include(x => x.Status)
                .Include(x => x.BankAccount)
                .OrderBy(x => x.RequestedAt)
                .ToListAsync();
        }
        catch (MySqlException ex) when (ex.Number == 1146
            && ex.Message.Contains("withdrawal", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex, "[WithdrawalRepository.GetPendingWithdrawalsAsync] withdrawal 관련 테이블이 없어 빈 목록 반환");
            return new List<DBModel.Withdrawal>();
        }
    }

    public async Task<DBModel.WithdrawalStatus?> GetStatusByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var trimmedCode = code.Trim();
        var normalizedCode = trimmedCode.ToUpperInvariant();

        try
        {
            return await context.WithdrawalStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == normalizedCode || x.Code == trimmedCode);
        }
        catch (MySqlException ex) when (ex.Number == 1146
            && ex.Message.Contains("withdrawal_status", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex, "[WithdrawalRepository.GetStatusByCodeAsync] withdrawal_status 테이블이 없어 null 반환");
            return null;
        }
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
