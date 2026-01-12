namespace TicketPlatFormServer.DTO.Chat;

public class GetMessagesReqDto
{
    public long RoomId { get; set; }
    public long UserId { get; set; }
    public long? LastMessageId { get; set; }
    public int Limit { get; set; } = 50;
}
