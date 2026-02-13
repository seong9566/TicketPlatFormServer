namespace TicketPlatFormServer.DTO.Dispute;

public class DisputeDetailRespDto
{
    public long Id { get; set; }
    public long TransactionId { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DisputeEvidenceRespDto> Evidences { get; set; } = new();
    public DisputeTransactionRespDto Transaction { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class DisputeEvidenceRespDto
{
    public long Id { get; set; }
    public string? ImageUrl { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DisputeTransactionRespDto
{
    public long TransactionId { get; set; }
    public string TicketTitle { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string BuyerNickname { get; set; } = string.Empty;
    public string SellerNickname { get; set; } = string.Empty;
}
