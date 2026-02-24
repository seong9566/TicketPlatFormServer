using TicketPlatFormServer.DTO.Reputation;

namespace TicketPlatFormServer.Services.Reputation;

public interface IReputationService
{
    Task<long> CreateAsync(long requestUserId, CreateReputationReqDto dto);
    Task<ReputationListRespDto> GetByUserIdAsync(long targetUserId, int page, int size);
    Task<ReputationCheckRespDto> CheckAsync(long requestUserId, long transactionId);
}
