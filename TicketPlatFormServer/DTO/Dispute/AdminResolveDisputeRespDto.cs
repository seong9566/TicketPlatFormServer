namespace TicketPlatFormServer.DTO.Dispute;

public class AdminResolveDisputeRespDto
{
    public long Id { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
