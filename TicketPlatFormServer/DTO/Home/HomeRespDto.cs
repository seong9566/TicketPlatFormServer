using System.Text.Json.Serialization;

namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 홈 화면 응답 DTO
/// </summary>
public class HomeRespDto
{
    /// <summary>
    /// 배너 목록
    /// </summary>
    public List<BannerDto> Banners { get; set; } = new();

    /// <summary>
    /// 카테고리 목록
    /// </summary>
    public List<CategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// 인기 공연 목록 (PopularEventList 섹션)
    /// </summary>
    public List<PopularEventDto> PopularEvents { get; set; } = new();

    /// <summary>
    /// 추천 공연 목록 (RecommendedEventList 섹션 - Just for you)
    /// </summary>
    public List<RecommendedEventDto> RecommendedEvents { get; set; } = new();

    [JsonPropertyName("deadlineDeals")]
    public List<DeadlineDealDto> DeadlineDeals { get; set; } = new();
}
