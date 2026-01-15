namespace TicketPlatFormServer.DTO.User;

/// <summary>
/// 사용자 프로필 응답 DTO
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// 사용자 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 이메일
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 닉네임
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 프로필 이미지 URL (Supabase Signed URL)
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// 자기소개
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// 매너 온도
    /// </summary>
    public float? MannerTemperature { get; set; }

    /// <summary>
    /// 총 거래 횟수
    /// </summary>
    public int? TotalTradeCount { get; set; }
}
