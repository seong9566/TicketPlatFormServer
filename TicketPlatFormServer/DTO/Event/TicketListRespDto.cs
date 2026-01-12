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
    /// 찜 여부 (userId가 제공된 경우만 값 설정)
    /// </summary>
    public bool? IsFavorited { get; set; }

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
    
    /// <summary>
    /// 총 거래 횟수
    /// </summary>
    public int TotalTradeCount { get; set; }
    
    /// <summary>
    /// 응답률 (0-100, 판매자가 채팅에 응답한 비율)
    /// </summary>
    public float? ResponseRate { get; set; }
    
    /// <summary>
    /// 안심결제 가능 여부 (본인인증, 휴대폰인증, 계좌인증 모두 완료)
    /// </summary>
    public bool IsSecurePayment { get; set; }
}
