using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Settlements;

public interface ISettlementRepository
{
    Task<List<DBModel.Settlement>> GetSettlementsBySellerIdAsync(long sellerId);

    Task<DBModel.Settlement?> GetSettlementByIdAsync(long id, long sellerId);

    Task<List<DBModel.Settlement>> GetDuePendingSettlementsAsync(DateTime now);

    Task UpdateSettlementAsync(DBModel.Settlement settlement);
}
