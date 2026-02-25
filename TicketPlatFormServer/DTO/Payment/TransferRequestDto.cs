namespace TicketPlatFormServer.DTO.Payment;

public class TransferRequestDto
{
    public string RefPayoutId { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public string ScheduleType { get; set; } = "EXPRESS";

    public string? PayoutDate { get; set; }

    public int Amount { get; set; }

    public string Currency { get; set; } = "KRW";

    public string TransactionDescription { get; set; } = "정산";

    public Dictionary<string, string>? Metadata { get; set; }
}
