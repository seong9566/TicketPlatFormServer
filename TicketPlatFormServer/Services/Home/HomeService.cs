using TicketPlatFormServer.DTO.Home;
using TicketPlatFormServer.Repository.Home;

namespace TicketPlatFormServer.Services.Home;

/// <summary>
/// 홈 화면 Service 구현체
/// </summary>
public class HomeService : IHomeService
{
    private readonly IHomeRepository _homeRepository;

    public HomeService(IHomeRepository homeRepository)
    {
        _homeRepository = homeRepository;
    }

    public async Task<HomeRespDto> GetHomeData(int? userId)
    {
        // 홈 화면 데이터 조회 (배너, 카테고리, 인기 공연, 추천 공연)
        var banners = await _homeRepository.GetBanners();
        var categories = await _homeRepository.GetCategories();
        var popularEvents = await _homeRepository.GetPopularEvents(10);
        var recommendedEvents = await _homeRepository.GetRecommendedEvents(userId, 5);

        return new HomeRespDto
        {
            Banners = banners,
            Categories = categories,
            PopularEvents = popularEvents,
            RecommendedEvents = recommendedEvents
        };
    }
} 

