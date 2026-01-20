namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 티켓 목록 조회 ReadModel (Repository 반환용)
/// </summary>
public class TicketListReadModel
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
    /// 좌석 등급 정렬 순서
    /// </summary>
    public int? SeatGradeSortOrder { get; set; }

    /// <summary>
    /// 구역 ID FK
    /// </summary>
    public int? AreaId { get; set; }

    /// <summary>
    /// 구역 (예: "A구역")
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// 구역 정렬 순서
    /// </summary>
    public int? AreaSortOrder { get; set; }

    /// <summary>
    /// 위치 ID FK
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// 위치명 (예: "1층", "2층", "플로어석")
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// 위치 정렬 순서
    /// </summary>
    public int? LocationSortOrder { get; set; }

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
    /// 티켓 특이사항 목록
    /// </summary>
    public List<TicketFeatureReadModel>? Features { get; set; }

    /// <summary>
    /// 판매자 정보
    /// </summary>
    public SellerInfoReadModel Seller { get; set; } = null!;
}

/// <summary>
/// 티켓 특징 ReadModel
/// </summary>
public class TicketFeatureReadModel
{
    public int FeatureId { get; set; }
    public string Code { get; set; } = null!;
    public string NameKo { get; set; } = null!;
}
