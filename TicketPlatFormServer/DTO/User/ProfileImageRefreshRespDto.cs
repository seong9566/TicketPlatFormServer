namespace TicketPlatFormServer.DTO.User;

/// <summary>
/// 프로필 이미지 URL 갱신 응답 DTO
/// </summary>
public class ProfileImageRefreshRespDto
{
    /// <summary>
    /// 새로 발급된 Signed URL (프로필 이미지가 없으면 null)
    /// </summary>
    public string? ProfileImageUrl { get; set; }
}
