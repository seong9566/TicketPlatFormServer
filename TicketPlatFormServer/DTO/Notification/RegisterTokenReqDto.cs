using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.Notification;

public class RegisterTokenReqDto
{
    [Required]
    public string DeviceToken { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(ANDROID|IOS)$")]
    public string Platform { get; set; } = string.Empty;
}
