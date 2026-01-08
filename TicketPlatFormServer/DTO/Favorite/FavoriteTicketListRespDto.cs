namespace TicketPlatFormServer.DTO.Favorite;

/// <summary>
/// 찜한 티켓 목록 응답 DTO
/// </summary>
public class FavoriteTicketListRespDto
{
    public int TicketId { get; set; }

    /// <summary>
    /// 티켓 제목
    /// </summary>
    public string TicketTitle { get; set; } = null!;

    /// <summary>
    /// 좌석 정보
    /// </summary>
    public string? SeatInfo { get; set; }

    /// <summary>
    /// 좌석 타입
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
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; set; }

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
