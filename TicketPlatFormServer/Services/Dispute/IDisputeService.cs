using TicketPlatFormServer.DTO.Dispute;

namespace TicketPlatFormServer.Services.Dispute;

public interface IDisputeService
{
    Task<DisputeSummaryRespDto> CreateDisputeAsync(long userId, CreateDisputeReqDto req);
    Task<DisputeListRespDto> GetMyDisputesAsync(long userId, string? cursor, int? limit);
    Task<DisputeDetailRespDto> GetDisputeDetailAsync(long userId, long disputeId);
    Task<AddDisputeEvidenceRespDto> AddEvidenceAsync(long userId, long disputeId, AddDisputeEvidenceReqDto req);
    Task<CancelDisputeRespDto> CancelDisputeAsync(long userId, long disputeId);
    Task<AdminResolveDisputeRespDto> ResolveDisputeAsync(long adminUserId, long disputeId, AdminResolveDisputeReqDto req);
    Task<AdminDisputeListRespDto> GetAllDisputesAsync(string? statusFilter, string? cursor, int? limit);
    Task<DisputeDetailRespDto> GetDisputeDetailForAdminAsync(long disputeId);
}
