namespace TicketPlatFormServer.DTO.Settlement;

public class SettlementResponseDto
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public int Amount { get; set; }

    public int Fee { get; set; }

    public int NetAmount { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? FailureReason { get; set; }
}
