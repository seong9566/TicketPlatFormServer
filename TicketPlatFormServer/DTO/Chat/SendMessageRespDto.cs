namespace TicketPlatFormServer.DTO.Chat;

public class SendMessageRespDto
{
    public long MessageId { get; set; }
    public long RoomId { get; set; }
    public string? Message { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? ImageUrlExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Success { get; set; }
}
