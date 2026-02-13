namespace TicketPlatFormServer.DTO.Dispute;

public class DisputeListRespDto
{
    public List<DisputeSummaryRespDto> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
