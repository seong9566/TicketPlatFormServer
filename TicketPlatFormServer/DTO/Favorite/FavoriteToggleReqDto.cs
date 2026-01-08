using System.Text.Json.Serialization;

namespace TicketPlatFormServer.DTO.Favorite;

/// <summary>
/// 티켓 찜 토글 요청 DTO
/// </summary>
public class FavoriteToggleReqDto
{
    /// <summary>
    /// 사용자 ID (서버에서 JWT Claims로부터 추출, 클라이언트는 전송하지 않음)
    /// </summary>
    [JsonIgnore]
    public int UserId { get; set; }

    /// <summary>
    /// 티켓 ID
    /// </summary>
    public int TicketId { get; set; }
}
