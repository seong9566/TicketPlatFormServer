namespace TicketPlatFormServer.DTO.Settlement;

public class SettlementListResponseDto
{
    public List<SettlementResponseDto> Settlements { get; set; } = [];

    public int TotalCount { get; set; }

    public long TotalNetAmount { get; set; }

    public SettlementSummaryDto Summary { get; set; } = new();
}
