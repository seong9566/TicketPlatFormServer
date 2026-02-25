using Microsoft.EntityFrameworkCore;

namespace TicketPlatFormServer.Repository.Settlements;

public class SettlementRepository(TicketContext context) : ISettlementRepository
{
    public async Task<List<DBModel.Settlement>> GetSettlementsBySellerIdAsync(long sellerId)
    {
        return await context.Settlements
            .Where(x => x.SellerId == sellerId)
            .Include(x => x.Status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<DBModel.Settlement?> GetSettlementByIdAsync(long id, long sellerId)
    {
        return await context.Settlements
            .Where(x => x.Id == id && x.SellerId == sellerId)
            .Include(x => x.Status)
            .FirstOrDefaultAsync();
    }

    public async Task<List<DBModel.Settlement>> GetDuePendingSettlementsAsync(DateTime now)
    {
        return await context.Settlements
            .Where(x => x.Status.Code == "pending" && x.ScheduledAt <= now)
            .Include(x => x.Status)
            .Include(x => x.BankAccount)
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync();
    }

    public async Task UpdateSettlementAsync(DBModel.Settlement settlement)
    {
        context.Settlements.Update(settlement);
        await context.SaveChangesAsync();
    }
}
