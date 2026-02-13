namespace TicketPlatFormServer.DTO.Dispute;

public class AddDisputeEvidenceRespDto
{
    public long Id { get; set; }
    public long DisputeId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
