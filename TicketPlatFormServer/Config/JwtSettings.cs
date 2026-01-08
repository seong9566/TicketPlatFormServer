namespace TicketPlatFormServer.Config;

/// <summary>
/// JWT 인증 설정
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// JWT 서명에 사용되는 비밀 키 (최소 256비트)
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// JWT 발급자 (Issuer)
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// JWT 대상자 (Audience)
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Access Token 만료 시간 (분 단위)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh Token 만료 시간 (일 단위)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// 서명 키 검증 여부
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// 발급자 검증 여부
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 대상자 검증 여부
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 토큰 수명 검증 여부
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 시간 차이 허용 범위 (초 단위)
    /// </summary>
    public int ClockSkew { get; set; } = 0;
}
