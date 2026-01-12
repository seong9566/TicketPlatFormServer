namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 티켓 목록 조회 ReadModel (Repository 반환용)
/// </summary>
public class TicketListReadModel
{
    public int TicketId { get; set; }

    /// <summary>
    /// 티켓 제목
    /// </summary>
    public string TicketTitle { get; set; } = null!;

    /// <summary>
    /// 좌석 정보 (예: "1층 5구역 3열")
    /// </summary>
    public string? SeatInfo { get; set; }

    /// <summary>
    /// 좌석 타입 (예: "VIP석", "R석", "S석")
    /// </summary>
    public string? SeatType { get; set; }

    /// <summary>
    /// 판매가
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 좌석 특징 (예: "연석", "통로석", "시야제한 없음")
    /// </summary>
    public List<string> SeatFeatures { get; set; } = new();

    /// <summary>
    /// 판매 사유/설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 이벤트 제목
    /// </summary>
    public string? EventTitle { get; set; }

    /// <summary>
    /// 공연 날짜
    /// </summary>
    public string? EventDate { get; set; }

    /// <summary>
    /// 장소명
    /// </summary>
    public string? VenueName { get; set; }

    /// <summary>
    /// 이벤트 포스터 이미지 URL
    /// </summary>
    public string? EventPosterImageUrl { get; set; }

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
    /// 판매자 정보
    /// </summary>
    public SellerInfoReadModel Seller { get; set; } = null!;
}
