using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Services.Event;

/// <summary>
/// 이벤트 관련 Service 인터페이스
/// </summary>
public interface IEventService
{ 
    /// <summary>
    /// 카테고리별 공연 목록 조회 (공연 기준)
    /// </summary>
    /// <param name="categoryId">카테고리 ID</param>
    /// <returns>공연 목록</returns>
    Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId);
    
    /// <summary>
    /// 이벤트 상세 정보 및 티켓 목록 조회
    /// </summary>
    /// <param name="eventId">이벤트 ID</param>
    /// <returns>이벤트 상세 정보 및 티켓 목록</returns>
    Task<EventDetailRespDto> GetEventDetailWithTickets(int eventId);
}

