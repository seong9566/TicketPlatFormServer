namespace TicketPlatFormServer.DTO.Payment;

public class TransferResponseDto
{
    public string? PayoutId { get; set; }

    public string? RefPayoutId { get; set; }

    public string? Status { get; set; }

    public string? RequestedAt { get; set; }

    public string? RawResponse { get; set; }
}
