using TicketPlatFormServer.DTO.Home;

namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// 홈 화면 Repository 인터페이스
/// </summary>
public interface IHomeRepository
{
    /// <summary>
    /// 인기 티켓 목록 조회
    /// </summary>
    Task<List<PopularTicketDto>> GetPopularTickets(int limit = 10);
    
    /// <summary>
    /// 추천 이벤트 목록 조회 (사용자 찜 기반)
    /// </summary>
    Task<List<RecommendedEventDto>> GetRecommendedEvents(int? userId, int limit = 5);
}

