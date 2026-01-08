using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Favorite;

/// <summary>
/// 찜 관련 Repository 인터페이스
/// </summary>
public interface IFavoriteRepository
{
    /// <summary>
    /// 찜 토글 (이미 찜한 경우 삭제, 아니면 추가)
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="favoriteTypeId">찜 유형 ID (2: 티켓 찜)</param>
    /// <param name="targetId">찜 대상 ID (티켓 ID)</param>
    /// <returns>찜 추가 여부 (true: 추가됨, false: 삭제됨)</returns>
    Task<bool> ToggleFavorite(int userId, int favoriteTypeId, int targetId);

    /// <summary>
    /// 티켓 존재 및 판매 가능 여부 확인
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>존재 및 판매 가능 여부</returns>
    Task<bool> CheckTicketExists(int ticketId);

    /// <summary>
    /// 사용자가 찜한 티켓 목록 조회 (이벤트 정보 포함)
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="favoriteTypeId">찜 유형 ID (2: 티켓 찜)</param>
    /// <returns>찜한 티켓 목록</returns>
    Task<List<FavoriteTicketReadModel>> GetFavoriteTicketsByUserId(int userId, int favoriteTypeId);

    /// <summary>
    /// 특정 티켓의 찜 여부 확인
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="favoriteTypeId">찜 유형 ID (2: 티켓 찜)</param>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>찜 여부</returns>
    Task<bool> CheckIsFavorited(int userId, int favoriteTypeId, int ticketId);
}
