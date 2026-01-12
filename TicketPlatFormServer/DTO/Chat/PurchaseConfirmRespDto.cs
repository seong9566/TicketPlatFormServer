namespace TicketPlatFormServer.DTO.Chat;

public class PurchaseConfirmRespDto
{
    public long TransactionId { get; set; }
    public DateTime ConfirmedAt { get; set; }
    public bool Success { get; set; }
}
