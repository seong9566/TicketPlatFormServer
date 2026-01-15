namespace TicketPlatFormServer.DTO.User;

/// <summary>
/// 프로필 이미지 URL 갱신 요청 DTO
/// </summary>
public class ProfileImageRefreshReqDto
{
    /// <summary>
    /// 대상 사용자 ID (null이면 본인)
    /// </summary>
    public int? UserId { get; set; }
}
