namespace TicketPlatFormServer.DTO.Chat;

public class ChatRoomListRespDto
{
    public long RoomId { get; set; }
    public long TicketId { get; set; }
    public string TicketTitle { get; set; } = null!;
    public OtherUserInfo OtherUser { get; set; } = null!;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public string RoomStatusCode { get; set; } = null!;
    public string RoomStatusName { get; set; } = null!;
    public long? TransactionId { get; set; }
    public string? TransactionStatusCode { get; set; }
    public string? TransactionStatusName { get; set; }
}

public class OtherUserInfo
{
    public long UserId { get; set; }
    public string Nickname { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
