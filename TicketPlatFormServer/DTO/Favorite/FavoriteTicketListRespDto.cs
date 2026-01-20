using TicketPlatFormServer.DTO.Ticket;

namespace TicketPlatFormServer.DTO.Favorite;

/// <summary>
/// 찜한 티켓 목록 응답 DTO
/// </summary>
public class FavoriteTicketListRespDto
{
    public int TicketId { get; set; }

    /// <summary>
    /// 좌석 등급 ID FK
    /// </summary>
    public int? SeatGradeId { get; set; }

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
    /// 판매가
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; set; }

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
    /// 티켓 등록 날짜
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 찜한 날짜
    /// </summary>
    public DateTime FavoritedAt { get; set; }

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
    /// 판매자 정보
    /// </summary>
    public SellerInfoDto Seller { get; set; } = null!;
}
