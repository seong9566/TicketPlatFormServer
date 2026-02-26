namespace TicketPlatFormServer.DTO;

/// <summary>
/// 비밀번호 찾기 요청 DTO
/// </summary>
public class ForgotPasswordReqDto
{
    /// <summary>
    /// 계정 이메일
    /// </summary>
    public string Email { get; set; } = null!;
}
