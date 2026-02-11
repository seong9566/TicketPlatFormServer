using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.Notification;

public class DeleteTokenReqDto
{
    [Required]
    public string DeviceToken { get; set; } = string.Empty;
}
