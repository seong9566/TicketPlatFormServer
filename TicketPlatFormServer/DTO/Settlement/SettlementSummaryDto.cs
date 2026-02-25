namespace TicketPlatFormServer.DTO.Settlement;

public class SettlementSummaryDto
{
    public int TotalAmount { get; set; }

    public int TotalFee { get; set; }

    public int TotalNetAmount { get; set; }

    public int PendingCount { get; set; }

    public int OnHoldCount { get; set; }

    public int ProcessingCount { get; set; }

    public int CompletedCount { get; set; }

    public int FailedCount { get; set; }
}
