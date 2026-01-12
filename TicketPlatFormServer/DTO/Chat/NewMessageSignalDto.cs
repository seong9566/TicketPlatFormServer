namespace TicketPlatFormServer.DTO.Chat;

public class NewMessageSignalDto
{
    public long MessageId { get; set; }
    public long RoomId { get; set; }
    public long SenderId { get; set; }
    public string SenderNickname { get; set; } = null!;
    public string? SenderProfileImage { get; set; }
    public string? Message { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
