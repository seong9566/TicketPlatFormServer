using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Disputes;

public interface IDisputeRepository
{
    Task SeedMasterDataIfMissingAsync();
    Task<DisputeType?> GetDisputeTypeByCodeAsync(string code);
    Task<DisputeStatus?> GetDisputeStatusByCodeAsync(string code);
    Task<bool> HasActiveDisputeAsync(long transactionId, IReadOnlyCollection<long> activeStatusIds);
    Task<Dispute> CreateDisputeAsync(Dispute dispute);
    Task<Dispute?> GetDisputeByIdWithDetailsAsync(long disputeId);
    Task<List<Dispute>> GetDisputesByClaimantCursorAsync(long claimantId, long? cursorId, int limitPlusOne);
    Task<Dictionary<long, int>> GetEvidenceCountMapAsync(IReadOnlyCollection<long> disputeIds);
    Task<DisputeEvidence> CreateEvidenceAsync(DisputeEvidence evidence);
    Task UpdateDisputeStatusAsync(long disputeId, long statusId);
    Task<List<Dispute>> GetAllDisputesCursorAsync(string? statusCode, long? cursorId, int limitPlusOne);
    Task<Dispute?> GetDisputeByIdAsync(long disputeId);
}
