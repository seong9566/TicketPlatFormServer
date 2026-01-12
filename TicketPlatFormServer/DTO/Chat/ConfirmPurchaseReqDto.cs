namespace TicketPlatFormServer.DTO.Chat;

public class ConfirmPurchaseReqDto
{
    public long RoomId { get; set; }
    public long TransactionId { get; set; }
    public long UserId { get; set; }
}
