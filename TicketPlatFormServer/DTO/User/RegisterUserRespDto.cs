namespace TicketPlatFormServer.DTO;

public class RegisterUserRespDto
{
    public string Email { get; set; }
    public string Phone { get; set; }
    /// <summary>
    /// 역할 코드 (예: "user", "admin")
    /// </summary>
    public string Role { get; set; }
    /// <summary>
    /// 가입 유형 코드 (예: "email", "kakao")
    /// </summary>
    public string Provider { get; set; }
}