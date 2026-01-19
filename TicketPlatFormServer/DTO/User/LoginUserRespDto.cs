namespace TicketPlatFormServer.DTO;

/// <summary>
/// 로그인 RespDto
/// </summary>
public class LoginUserRespDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    /// <summary>
    /// 역할 코드 (예: "user", "admin")
    /// </summary>
    public string Role { get; set; } = null!;
    /// <summary>
    /// 가입 유형 코드 (예: "email", "kakao")
    /// </summary>
    public string Provider { get; set; } = null!;
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Access Token (Bearer Token)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh Token (UUID 형식)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token 만료 시간 (초 단위)
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Token 타입 (기본값: Bearer)
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Token 만료 일시 (UTC)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
