namespace TicketPlatFormServer.DTO.Dispute;

public class DisputeSummaryRespDto
{
    public long Id { get; set; }
    public long TransactionId { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EvidenceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
