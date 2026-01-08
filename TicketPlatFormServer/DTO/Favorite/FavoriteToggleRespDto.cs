namespace TicketPlatFormServer.DTO.Favorite;

/// <summary>
/// 티켓 찜 토글 응답 DTO
/// </summary>
public class FavoriteToggleRespDto
{
    /// <summary>
    /// 티켓 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 찜 여부 (true: 찜됨, false: 찜 해제됨)
    /// </summary>
    public bool IsFavorited { get; set; }
}
