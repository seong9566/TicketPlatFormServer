using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO;

/// <summary>
/// 로그인 ReqDto
/// </summary>
public class LoginUserReqDto
{
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

