using TicketPlatFormServer.DTO.Settlement;

namespace TicketPlatFormServer.Services.Settlements;

public interface ISettlementService
{
    Task ProcessPendingSettlementsAsync();

    Task<SettlementListResponseDto> GetBySellerAsync(long sellerId, int page, int pageSize, string? statusFilter);

    Task<SettlementDetailRespDto> GetDetailAsync(long settlementId, long sellerId);

    Task<SettlementListResponseDto> GetMySettlementsAsync(long sellerId);

    Task<SettlementResponseDto?> GetSettlementByIdAsync(long id, long sellerId);
}
