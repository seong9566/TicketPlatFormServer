using TicketPlatFormServer.DTO.Favorite;

namespace TicketPlatFormServer.Services.Favorite;

/// <summary>
/// 찜 관련 Service 인터페이스
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// 티켓 찜 토글 (추가/삭제)
    /// </summary>
    /// <param name="req">티켓 찜 요청 DTO</param>
    /// <returns>찜 상태 결과</returns>
    Task<FavoriteToggleRespDto> ToggleTicketFavorite(FavoriteToggleReqDto req);

    /// <summary>
    /// 사용자가 찜한 티켓 목록 조회
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>찜한 티켓 목록</returns>
    Task<List<FavoriteTicketListRespDto>> GetFavoriteTicketsByUserId(int userId);
}
