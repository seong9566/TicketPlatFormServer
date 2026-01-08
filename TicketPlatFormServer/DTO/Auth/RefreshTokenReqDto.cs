namespace TicketPlatFormServer.DTO;

/// <summary>
/// Refresh Token 요청 DTO
/// </summary>
public class RefreshTokenReqDto
{
    /// <summary>
    /// Refresh Token (UUID 형식)
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}
