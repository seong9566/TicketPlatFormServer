namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 판매 대시보드 응답 DTO (페이징)
/// </summary>
public class SalesDashboardRespDto
{
    /// <summary>
    /// 이벤트 그룹 목록
    /// </summary>
    public List<EventGroupItemDto> EventGroups { get; set; } = new();

    /// <summary>
    /// 현재 페이지
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 페이지 크기
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 전체 이벤트 수
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 다음 페이지 존재 여부
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// 판매 대시보드 이벤트 그룹 아이템
/// </summary>
public class EventGroupItemDto
{
    /// <summary>
    /// 이벤트 ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 이벤트 제목
    /// </summary>
    public string EventTitle { get; set; } = null!;

    /// <summary>
    /// 포스터 이미지 URL
    /// </summary>
    public string? PosterImageUrl { get; set; }

    /// <summary>
    /// 공연장 이름
    /// </summary>
    public string? VenueName { get; set; }

    /// <summary>
    /// 가장 빠른 공연 일시
    /// </summary>
    public DateTime? EarliestEventDatetime { get; set; }

    /// <summary>
    /// 전체 티켓 수
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 판매 중 티켓 수
    /// </summary>
    public int OnSaleCount { get; set; }

    /// <summary>
    /// 판매 완료 티켓 수
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 정산 중 티켓 수
    /// </summary>
    public int SettlingCount { get; set; }

    public string? RepresentativeSeatInfo { get; set; }
}
