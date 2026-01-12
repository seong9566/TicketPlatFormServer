namespace TicketPlatFormServer.DTO.Chat;

public class PaymentUrlRespDto
{
    public string PaymentUrl { get; set; } = null!;
    public long TransactionId { get; set; }
    public int Amount { get; set; }
}
