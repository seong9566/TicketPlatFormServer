using TicketPlatFormServer.DTO.Home;

namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// 홈 화면 Repository 인터페이스
/// </summary>
public interface IHomeRepository
{
    /// <summary>
    /// 배너 목록 조회
    /// </summary>
    Task<List<BannerDto>> GetBanners();

    /// <summary>
    /// 카테고리 목록 조회
    /// </summary>
    Task<List<CategoryDto>> GetCategories();

    /// <summary>
    /// 인기 공연 목록 조회
    /// </summary>
    Task<List<PopularEventDto>> GetPopularEvents(int limit = 10);

    /// <summary>
    /// 추천 이벤트 목록 조회 (사용자 찜 기반)
    /// </summary>
    Task<List<RecommendedEventDto>> GetRecommendedEvents(int? userId, int limit = 5);

    Task<List<DeadlineDealDto>> GetDeadlineDeals(int limit = 10);
}
