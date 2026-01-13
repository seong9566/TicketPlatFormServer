namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 판매용 공연 목록 응답 DTO (페이징)
/// </summary>
public class SellEventListRespDto
{
    /// <summary>
    /// 공연 목록
    /// </summary>
    public List<SellEventItem> Events { get; set; } = new();

    /// <summary>
    /// 전체 공연 수
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 현재 페이지
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 페이지 크기
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 전체 페이지 수
    /// </summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// 공연 아이템
/// </summary>
public class SellEventItem
{
    /// <summary>
    /// 공연 ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 공연 제목
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 포스터 이미지 URL
    /// </summary>
    public string? PosterImageUrl { get; set; }

    /// <summary>
    /// 장소명
    /// </summary>
    public string? VenueName { get; set; }

    /// <summary>
    /// 공연 시작 일시
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 공연 종료 일시
    /// </summary>
    public DateTime? EndAt { get; set; }
}
