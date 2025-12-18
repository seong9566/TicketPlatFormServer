using TicketPlatFormServer.DTO;

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
    Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId);
}

