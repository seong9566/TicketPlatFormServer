namespace TicketPlatFormServer.DTO;

/// <summary>
/// 티켓 목록 조회 RespDto
/// </summary>
public class TicketListRespDto
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
    /// 티켓 등록 날짜
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 판매자 정보
    /// </summary>
    public SellerInfoDto Seller { get; set; } = null!;
}

/// <summary>
/// 판매자 정보 Dto
/// </summary>
public class SellerInfoDto
{
    public int UserId { get; set; }
    
    /// <summary>
    /// 닉네임
    /// </summary>
    public string Nickname { get; set; } = null!;
    
    /// <summary>
    /// 프로필 이미지 URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }
    
    /// <summary>
    /// 매너 온도
    /// </summary>
    public float? MannerTemperature { get; set; }
}
