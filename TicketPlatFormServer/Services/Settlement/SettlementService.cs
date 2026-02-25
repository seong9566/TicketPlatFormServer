using TicketPlatFormServer.DTO.Settlement;
using TicketPlatFormServer.Repository.Settlements;

namespace TicketPlatFormServer.Services.Settlements;

public class SettlementService(ISettlementRepository settlementRepository) : ISettlementService
{
    public async Task<SettlementListResponseDto> GetMySettlementsAsync(long sellerId)
    {
        var settlements = await settlementRepository.GetSettlementsBySellerIdAsync(sellerId);

        var items = settlements.Select(ToResponse).ToList();
        var summary = new SettlementSummaryDto
        {
            TotalAmount = settlements.Sum(x => x.Amount),
            TotalFee = settlements.Sum(x => x.Fee),
            TotalNetAmount = settlements.Sum(x => x.NetAmount),
            PendingCount = settlements.Count(x => x.Status.Code == "pending"),
            OnHoldCount = settlements.Count(x => x.Status.Code == "on_hold"),
            ProcessingCount = settlements.Count(x => x.Status.Code == "processing"),
            CompletedCount = settlements.Count(x => x.Status.Code == "completed"),
            FailedCount = settlements.Count(x => x.Status.Code == "failed")
        };

        return new SettlementListResponseDto
        {
            Settlements = items,
            TotalCount = items.Count,
            Summary = summary
        };
    }

    public async Task<SettlementResponseDto?> GetSettlementByIdAsync(long id, long sellerId)
    {
        var settlement = await settlementRepository.GetSettlementByIdAsync(id, sellerId);
        return settlement == null ? null : ToResponse(settlement);
    }

    private static SettlementResponseDto ToResponse(DBModel.Settlement settlement)
    {
        return new SettlementResponseDto
        {
            Id = settlement.Id,
            TransactionId = settlement.TransactionId,
            Amount = settlement.Amount,
            Fee = settlement.Fee,
            NetAmount = settlement.NetAmount,
            StatusCode = settlement.Status.Code,
            StatusName = settlement.Status.NameKo ?? settlement.Status.Code,
            ScheduledAt = settlement.ScheduledAt,
            ProcessedAt = settlement.ProcessedAt,
            FailureReason = settlement.FailureReason
        };
    }
}
