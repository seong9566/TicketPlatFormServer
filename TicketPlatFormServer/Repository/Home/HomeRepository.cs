using System.Data;
using Dapper;
using TicketPlatFormServer.DTO.Home;

namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// 홈 화면 Repository 구현체 (Primary Constructor + Static Class 패턴)
/// </summary>
public class HomeRepository(IDbConnection dapper) : IHomeRepository
{
    public async Task<List<BannerDto>> GetBanners()
    {
        var result = await dapper.QueryAsync<BannerDto>(HomeQueries.GetBanners);
        return result.ToList();
    }

    public async Task<List<CategoryDto>> GetCategories()
    {
        var result = await dapper.QueryAsync<CategoryDto>(HomeQueries.GetCategories);

        // 카테고리 코드에 따른 아이콘 매핑 (임시)
        var categories = result.ToList();
        foreach (var category in categories)
        {
            category.IconName = MapCategoryCodeToIcon(category.IconName);
        }

        return categories;
    }

    public async Task<List<PopularEventDto>> GetPopularEvents(int limit = 10)
    {
        var result = await dapper.QueryAsync<PopularEventDto>(
            HomeQueries.GetPopularEvents,
            new { Limit = limit }
        );

        return result.ToList();
    }

    public async Task<List<RecommendedEventDto>> GetRecommendedEvents(int? userId = null, int limit = 5)
    {
        var sql = userId.HasValue
            ? HomeQueries.GetRecommendedEventsForUser
            : HomeQueries.GetRecommendedEventsForGuest;

        var result = await dapper.QueryAsync<RecommendedEventDto>(
            sql,
            new { UserId = userId, Limit = limit }
        );

        return result.ToList();
    }

    /// <summary>
    /// 카테고리 코드를 Material Icon 이름으로 매핑
    /// </summary>
    private string MapCategoryCodeToIcon(string code) => code.ToLower() switch
    {
        "concert" => "music_note_outlined",
        "musical" => "theater_comedy",
        "sports" => "emoji_events_outlined",
        "exhibition" => "palette_outlined",
        _ => "event"
    };
}
