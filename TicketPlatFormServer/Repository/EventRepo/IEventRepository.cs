using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.EventRepo;

/// <summary>
/// 이벤트 관련 Repository 인터페이스
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// 카테고리별 공연 목록 조회 (공연 기준)
    /// </summary>
    /// <param name="categoryId">카테고리 ID</param>
    /// <returns>공연 목록</returns>
    Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId);

    /// <summary>
    /// 이벤트 상세 정보 조회 (이벤트 정보만, 티켓 목록 제외)
    /// </summary>
    /// <param name="eventId">이벤트 ID</param>
    /// <returns>이벤트 상세 정보</returns>
    Task<EventDetailReadModel?> GetEventDetailById(int eventId);
}

