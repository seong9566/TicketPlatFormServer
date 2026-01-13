namespace TicketPlatFormServer.DTO.Chat;

public class ChatMessageRespDto
{
    public long MessageId { get; set; }
    public long RoomId { get; set; }
    public int SenderId { get; set; }
    public string SenderNickname { get; set; } = null!;
    public string? SenderProfileImage { get; set; }
    public string? Message { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? ImageUrlExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMyMessage { get; set; }
}
