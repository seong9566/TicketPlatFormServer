using System.ComponentModel.DataAnnotations;
using TicketPlatFormServer.Enum;

namespace TicketPlatFormServer.DTO;

/// <summary>
/// 회원가입 ReqDto
/// </summary>
public class RegisterUserReqDto
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? Phone { get; set; }

    public string Role { get; set; } = "User";
    
    [Required]
    public string Provider { get; set; } = "Email";

}