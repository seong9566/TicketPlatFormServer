using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Settlements;

public class SettlementRepository(TicketContext context, IDbConnection dapper) : ISettlementRepository
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

    public async Task<IEnumerable<SettlementListReadModel>> GetBySellerIdAsync(
        long sellerId, int page, int pageSize, string? statusFilter)
    {
        var offset = (page - 1) * pageSize;
        return await dapper.QueryAsync<SettlementListReadModel>(
            SettlementQueries.GetBySellerIdList,
            new { SellerId = sellerId, StatusFilter = statusFilter, PageSize = pageSize, Offset = offset });
    }

    public async Task<int> CountBySellerIdAsync(long sellerId, string? statusFilter)
    {
        return await dapper.ExecuteScalarAsync<int>(
            SettlementQueries.CountBySellerId,
            new { SellerId = sellerId, StatusFilter = statusFilter });
    }

    public async Task<long> GetTotalCompletedNetAmountAsync(long sellerId)
    {
        return await dapper.ExecuteScalarAsync<long>(
            SettlementQueries.GetTotalCompletedNetAmount,
            new { SellerId = sellerId });
    }

    public async Task<SettlementDetailReadModel?> GetDetailByIdAndSellerIdAsync(
        long settlementId, long sellerId)
    {
        return await dapper.QueryFirstOrDefaultAsync<SettlementDetailReadModel>(
            SettlementQueries.GetDetailByIdAndSellerId,
            new { SettlementId = settlementId, SellerId = sellerId });
    }

    public async Task<bool> HasBalanceTransactionAsync(long settlementId)
    {
        var result = await dapper.ExecuteScalarAsync<int>(
            SettlementQueries.HasBalanceTransaction,
            new { SettlementId = settlementId });
        return result != 0;
    }
}
