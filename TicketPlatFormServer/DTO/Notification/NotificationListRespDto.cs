namespace TicketPlatFormServer.DTO.Notification;

public class NotificationListRespDto
{
    public List<NotificationItemRespDto> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
