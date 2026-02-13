using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Disputes;

public class DisputeRepository(TicketContext context) : IDisputeRepository
{
    private static readonly (string Code, string NameKo, int SortOrder)[] RequiredTypes =
    [
        ("FAKE_TICKET", "가짜/위조 티켓", 1),
        ("WRONG_TICKET", "잘못된 티켓", 2),
        ("NO_DELIVERY", "티켓 미배송", 3),
        ("RUDE_BEHAVIOR", "비매너 행위", 4),
        ("OTHER", "기타", 5)
    ];

    private static readonly (string Code, string NameKo, int SortOrder)[] RequiredStatuses =
    [
        ("PENDING", "접수 대기", 1),
        ("IN_REVIEW", "검토 중", 2),
        ("RESOLVED_BUYER", "구매자 승", 3),
        ("RESOLVED_SELLER", "판매자 승", 4),
        ("REJECTED", "신고 기각", 5),
        ("CANCELLED", "신고자 취소", 6)
    ];

    public async Task SeedMasterDataIfMissingAsync()
    {
        var hasChanges = false;

        var existingTypeCodes = await context.DisputeTypes
            .Select(x => x.Code)
            .ToListAsync();
        var nextTypeId = await context.DisputeTypes.AnyAsync()
            ? await context.DisputeTypes.MaxAsync(x => x.Id) + 1
            : 1;

        foreach (var requiredType in RequiredTypes)
        {
            if (existingTypeCodes.Contains(requiredType.Code))
            {
                continue;
            }

            context.DisputeTypes.Add(new DisputeType
            {
                Id = nextTypeId++,
                Code = requiredType.Code,
                NameKo = requiredType.NameKo,
                IsActive = true,
                SortOrder = requiredType.SortOrder
            });
            hasChanges = true;
        }

        var existingStatusCodes = await context.DisputeStatuses
            .Select(x => x.Code)
            .ToListAsync();
        var nextStatusId = await context.DisputeStatuses.AnyAsync()
            ? await context.DisputeStatuses.MaxAsync(x => x.Id) + 1
            : 1;

        foreach (var requiredStatus in RequiredStatuses)
        {
            if (existingStatusCodes.Contains(requiredStatus.Code))
            {
                continue;
            }

            context.DisputeStatuses.Add(new DisputeStatus
            {
                Id = nextStatusId++,
                Code = requiredStatus.Code,
                NameKo = requiredStatus.NameKo,
                IsActive = true,
                SortOrder = requiredStatus.SortOrder
            });
            hasChanges = true;
        }

        var hasFrozenEscrowStatus = await context.EscrowStatuses
            .AnyAsync(x => x.Code == "frozen");

        if (!hasFrozenEscrowStatus)
        {
            var nextEscrowStatusId = await context.EscrowStatuses.AnyAsync()
                ? await context.EscrowStatuses.MaxAsync(x => x.Id) + 1
                : 1;

            context.EscrowStatuses.Add(new EscrowStatus
            {
                Id = nextEscrowStatusId,
                Code = "frozen",
                NameKo = "동결",
                IsActive = true,
                SortOrder = 4
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync();
        }
    }

    public async Task<DisputeType?> GetDisputeTypeByCodeAsync(string code)
    {
        return await context.DisputeTypes
            .FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
    }

    public async Task<DisputeStatus?> GetDisputeStatusByCodeAsync(string code)
    {
        return await context.DisputeStatuses
            .FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
    }

    public async Task<bool> HasActiveDisputeAsync(long transactionId, IReadOnlyCollection<long> activeStatusIds)
    {
        if (activeStatusIds.Count == 0)
        {
            return false;
        }

        return await context.Disputes
            .AnyAsync(x => x.TransactionId == transactionId && activeStatusIds.Contains(x.StatusId));
    }

    public async Task<Dispute> CreateDisputeAsync(Dispute dispute)
    {
        dispute.CreatedAt = DateTime.UtcNow;
        context.Disputes.Add(dispute);
        await context.SaveChangesAsync();

        await context.Entry(dispute).Reference(x => x.Type).LoadAsync();
        await context.Entry(dispute).Reference(x => x.Status).LoadAsync();
        return dispute;
    }

    public async Task<Dispute?> GetDisputeByIdWithDetailsAsync(long disputeId)
    {
        return await context.Disputes
            .Include(x => x.Type)
            .Include(x => x.Status)
            .Include(x => x.DisputeEvidences)
            .Include(x => x.Transaction)
                .ThenInclude(t => t.TransactionItems)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == disputeId);
    }

    public async Task<List<Dispute>> GetDisputesByClaimantCursorAsync(long claimantId, long? cursorId, int limitPlusOne)
    {
        var query = context.Disputes
            .AsNoTracking()
            .Include(x => x.Type)
            .Include(x => x.Status)
            .Where(x => x.ClaimantId == claimantId);

        if (cursorId.HasValue)
        {
            query = query.Where(x => x.Id < cursorId.Value);
        }

        return await query
            .OrderByDescending(x => x.Id)
            .Take(limitPlusOne)
            .ToListAsync();
    }

    public async Task<Dictionary<long, int>> GetEvidenceCountMapAsync(IReadOnlyCollection<long> disputeIds)
    {
        if (disputeIds.Count == 0)
        {
            return new Dictionary<long, int>();
        }

        return await context.DisputeEvidences
            .Where(x => disputeIds.Contains(x.DisputeId))
            .GroupBy(x => x.DisputeId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
    }

    public async Task<DisputeEvidence> CreateEvidenceAsync(DisputeEvidence evidence)
    {
        evidence.CreatedAt = DateTime.UtcNow;
        context.DisputeEvidences.Add(evidence);
        await context.SaveChangesAsync();
        return evidence;
    }

    public async Task UpdateDisputeStatusAsync(long disputeId, long statusId)
    {
        await context.Disputes
            .Where(x => x.Id == disputeId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.StatusId, statusId));
    }
}
