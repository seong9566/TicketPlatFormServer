namespace TicketPlatFormServer.DTO.Ticket;

/// <summary>
/// 티켓 상세 조회 RespDto
/// </summary>
public class TicketDetailRespDto
{
    public int TicketId { get; set; }

    /// <summary>
    /// 좌석 등급 ID FK
    /// </summary>
    public int? SeatGradeId { get; set; }

    /// <summary>
    /// 좌석 등급 코드 (예: "VIP", "R", "S")
    /// </summary>
    public string? SeatGradeCode { get; set; }

    /// <summary>
    /// 좌석 등급 이름 (예: "VIP석", "일반석")
    /// </summary>
    public string? SeatGradeName { get; set; }

    /// <summary>
    /// 좌석 등급 영문명 (예: "VIP Seat")
    /// </summary>
    public string? SeatGradeNameEn { get; set; }

    /// <summary>
    /// 구역 ID FK
    /// </summary>
    public int? AreaId { get; set; }

    /// <summary>
    /// 구역 (예: "A구역")
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// 위치 ID FK
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// 위치명 (예: "1층", "2층", "플로어석")
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// 열 (예: "5열")
    /// </summary>
    public string? Row { get; set; }

    /// <summary>
    /// 판매가
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 연석 여부
    /// </summary>
    public bool? IsConsecutive { get; set; }

    /// <summary>
    /// 거래 방법 ID FK
    /// </summary>
    public int? TradeMethodId { get; set; }

    /// <summary>
    /// 거래 방법 이름 (예: "PIN거래", "배송거래")
    /// </summary>
    public string? TradeMethodName { get; set; }

    /// <summary>
    /// 티켓 보유 여부
    /// </summary>
    public bool? HasTicket { get; set; }

    /// <summary>
    /// 판매 사유/설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 티켓 등록 날짜
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 티켓 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// 1인 1매 여부 (quantity가 1이면 true)
    /// </summary>
    public bool IsSingleTicket { get; set; }

    /// <summary>
    /// 티켓 이미지 URL 목록
    /// </summary>
    public List<string> TicketImages { get; set; } = new();

    /// <summary>
    /// 찜 여부 (userId가 제공된 경우만 값 설정)
    /// </summary>
    public bool? IsFavorited { get; set; }

    /// <summary>
    /// 티켓 특이사항 목록
    /// </summary>
    public List<TicketFeatureDto>? Features { get; set; }

    /// <summary>
    /// 판매자 정보
    /// </summary>
    public SellerInfoDto Seller { get; set; } = null!;

    /// <summary>
    /// 이벤트 정보
    /// </summary>
    public EventInfoDto Event { get; set; } = null!;
}

/// <summary>
/// 이벤트 정보 Dto (티켓 상세 조회용)
/// </summary>
public class EventInfoDto
{
    public int EventId { get; set; }

    /// <summary>
    /// 이벤트 제목
    /// </summary>
    public string EventTitle { get; set; } = null!;

    /// <summary>
    /// 포스터 이미지 URL (Signed URL)
    /// </summary>
    public string? PosterImageUrl { get; set; }

    /// <summary>
    /// 공연 시작 일시
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 공연 종료 일시
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 공연 장소명
    /// </summary>
    public string? VenueName { get; set; }
}
