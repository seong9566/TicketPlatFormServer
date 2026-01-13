namespace TicketPlatFormServer.DTO.Chat;

public class RefreshImageUrlRespDto
{
    public long MessageId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
