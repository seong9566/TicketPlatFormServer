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
        // 인기 티켓과 추천 이벤트 조회 
        var popularTickets = await _homeRepository.GetPopularTickets(10);
        var recommendedEvents = await _homeRepository.GetRecommendedEvents(userId, 5);

        return new HomeRespDto
        {
            PopularTickets = popularTickets,
            RecommendedEvents = recommendedEvents
        };
    }
} 

