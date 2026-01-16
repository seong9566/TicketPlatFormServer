namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 내 판매 티켓 목록 응답 DTO (페이징)
/// </summary>
public class MyTicketListRespDto
{
    /// <summary>
    /// 티켓 목록
    /// </summary>
    public List<MyTicketItem> Tickets { get; set; } = new();

    /// <summary>
    /// 전체 티켓 수
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
/// 내 판매 티켓 아이템
/// </summary>
public class MyTicketItem
{
    /// <summary>
    /// 티켓 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 티켓 제목
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 공연 일시
    /// </summary>
    public DateTime EventDatetime { get; set; }

    /// <summary>
    /// 좌석 등급 이름 (예: "VIP석", "일반석")
    /// </summary>
    public string? SeatGradeName { get; set; }

    /// <summary>
    /// 구역 (예: "A구역")
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// 열 (예: "5열")
    /// </summary>
    public string? Row { get; set; }

    /// <summary>
    /// 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// 판매가
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 상태
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 등록일
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 대표 이미지 URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}
