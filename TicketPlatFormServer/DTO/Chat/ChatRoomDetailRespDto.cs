namespace TicketPlatFormServer.DTO.Chat;

public class ChatRoomDetailRespDto
{
    public long RoomId { get; set; }
    public TicketInfo Ticket { get; set; } = null!;
    public UserInfo Buyer { get; set; } = null!;
    public UserInfo Seller { get; set; } = null!;
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public TransactionInfo? Transaction { get; set; }
    public bool CanSendMessage { get; set; }
    public bool CanRequestPayment { get; set; }
    public bool CanConfirmPurchase { get; set; }
    public bool CanCancelTransaction { get; set; }
    public List<ChatMessageRespDto> Messages { get; set; } = new();
}

public class TicketInfo
{
    public int TicketId { get; set; }
    public string Title { get; set; } = null!;
    public int Price { get; set; }
    /// <summary>
    /// 티켓 1장당 가격
    /// </summary>
    public int UnitPrice { get; set; }
    /// <summary>
    /// 총 판매중인 수량
    /// </summary>
    public int TotalQuantity { get; set; }
    /// <summary>
    /// 남은 판매 가능 수량
    /// </summary>
    public int RemainingQuantity { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// 좌석 정보 (예: "1층 VIP A구역 3열")
    /// </summary>
    public string? SeatInfo { get; set; }
    
    /// <summary>
    /// 공연 일시
    /// </summary>
    public DateTime? EventDateTime { get; set; }
    
    /// <summary>
    /// 공연장 이름
    /// </summary>
    public string? VenueName { get; set; }
}

public class UserInfo
{
    public int UserId { get; set; }
    public string Nickname { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public double MannerTemperature { get; set; }
}

public class TransactionInfo
{
    public long TransactionId { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public int? Amount { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
