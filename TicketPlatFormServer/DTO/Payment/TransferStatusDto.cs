namespace TicketPlatFormServer.DTO.Payment;

public class TransferStatusDto
{
    public string? PayoutId { get; set; }

    public string? Status { get; set; }

    public string? FailureReason { get; set; }

    public string? RawResponse { get; set; }
}
