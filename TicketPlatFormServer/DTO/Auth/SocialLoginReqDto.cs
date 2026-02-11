using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO;

public class SocialLoginReqDto
{
    [Required]
    [RegularExpression("^(google|kakao)$")]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string AccessToken { get; set; } = string.Empty;
}
