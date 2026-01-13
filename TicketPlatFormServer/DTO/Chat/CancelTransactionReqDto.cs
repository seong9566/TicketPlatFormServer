namespace TicketPlatFormServer.DTO.Chat;

public class CancelTransactionReqDto
{
    public long RoomId { get; set; }
    public long TransactionId { get; set; }
    public string CancelReason { get; set; } = null!;
    public int UserId { get; set; }
}
