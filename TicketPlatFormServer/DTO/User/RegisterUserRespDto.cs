namespace TicketPlatFormServer.DTO;

public class RegisterUserRespDto
{
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    /// <summary>
    /// 역할 코드 (예: "user", "admin")
    /// </summary>
    public string Role { get; set; } = null!;
    /// <summary>
    /// 가입 유형 코드 (예: "email", "kakao")
    /// </summary>
    public string Provider { get; set; } = null!;
}
