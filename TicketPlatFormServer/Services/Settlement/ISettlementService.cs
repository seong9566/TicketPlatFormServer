using TicketPlatFormServer.DTO.Settlement;

namespace TicketPlatFormServer.Services.Settlements;

public interface ISettlementService
{
    Task<SettlementListResponseDto> GetMySettlementsAsync(long sellerId);

    Task<SettlementResponseDto?> GetSettlementByIdAsync(long id, long sellerId);
}
