using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Settlements;

public interface ISettlementRepository
{
    Task<List<DBModel.Settlement>> GetSettlementsBySellerIdAsync(long sellerId);

    Task<DBModel.Settlement?> GetSettlementByIdAsync(long id, long sellerId);

    Task<List<DBModel.Settlement>> GetDuePendingSettlementsAsync(DateTime now);

    Task UpdateSettlementAsync(DBModel.Settlement settlement);

    Task<IEnumerable<SettlementListReadModel>> GetBySellerIdAsync(long sellerId, int page, int pageSize, string? statusFilter);

    Task<int> CountBySellerIdAsync(long sellerId, string? statusFilter);

    Task<long> GetTotalCompletedNetAmountAsync(long sellerId);

    Task<SettlementDetailReadModel?> GetDetailByIdAndSellerIdAsync(long settlementId, long sellerId);

    Task<bool> HasBalanceTransactionAsync(long settlementId);
}
