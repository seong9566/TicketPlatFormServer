namespace TicketPlatFormServer.DTO.Dispute;

public class AdminDisputeListRespDto
{
    public List<AdminDisputeListItemDto> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}

public class AdminDisputeListItemDto
{
    public long Id { get; set; }
    public long TransactionId { get; set; }
    public string? ClaimantNickname { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int EvidenceCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
