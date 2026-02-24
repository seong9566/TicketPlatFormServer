namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 이벤트 티켓 목록 응답 DTO (페이징)
/// </summary>
public class EventTicketListRespDto
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
    /// 티켓 목록
    /// </summary>
    public List<EventTicketItemDto> Tickets { get; set; } = new();

    /// <summary>
    /// 현재 페이지
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 페이지 크기
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 전체 티켓 수
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 다음 페이지 존재 여부
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// 이벤트 티켓 아이템
/// </summary>
public class EventTicketItemDto
{
    /// <summary>
    /// 티켓 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 좌석 정보
    /// </summary>
    public string? SeatInfo { get; set; }

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
    /// 상태 코드
    /// </summary>
    public string StatusCode { get; set; } = null!;

    /// <summary>
    /// 상태 이름
    /// </summary>
    public string StatusName { get; set; } = null!;

    /// <summary>
    /// 거래 ID
    /// </summary>
    public long? TransactionId { get; set; }

    /// <summary>
    /// 썸네일 이미지 URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 등록일
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
