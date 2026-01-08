namespace TicketPlatFormServer.DTO;

/// <summary>
/// Token 응답 DTO
/// </summary>
public class TokenResponseDto
{
    /// <summary>
    /// Access Token (Bearer Token)
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Refresh Token (UUID 형식)
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Token 만료 시간 (초 단위)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token 타입 (기본값: Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token 만료 일시 (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
