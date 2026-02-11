namespace TicketPlatFormServer.DTO.Notification;

public class RegisterTokenRespDto
{
    public long Id { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
