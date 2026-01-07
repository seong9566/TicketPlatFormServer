namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 인기 공연 DTO (PopularEventList 섹션)
/// </summary>
public class PopularEventDto
{
    /// <summary>
    /// 공연 ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 공연 제목
    /// </summary>
    public string EventTitle { get; set; } = null!;

    /// <summary>
    /// 공연 소개/설명 텍스트
    /// </summary>
    public string? EventDescription { get; set; }

    /// <summary>
    /// 공연 날짜 (예: "2026.05.20")
    /// </summary>
    public string EventDate { get; set; } = null!;

    /// <summary>
    /// 공연 장소
    /// </summary>
    public string Venue { get; set; } = null!;

    /// <summary>
    /// 공연에서 가장 저렴한 티켓 가격 (원)
    /// </summary>
    public int MinTicketPrice { get; set; }

    /// <summary>
    /// 공연의 가장 저렴한 티켓의 원가 (원)
    /// </summary>
    public int OriginalMinTicketPrice { get; set; }

    /// <summary>
    /// 공연의 티켓 할인율 (%). 예: 8 → UI에서 "-8%"
    /// </summary>
    public int TicketDiscountRate { get; set; }

    /// <summary>
    /// 공연 포스터 이미지 URL
    /// </summary>
    public string? PosterImageUrl { get; set; }

    /// <summary>
    /// 공연 티켓의 판매 가능 티켓 수량
    /// </summary>
    public int AvailableTicketCount { get; set; }

    /// <summary>
    /// 카테고리 ID
    /// </summary>
    public int CategoryId { get; set; }
}
