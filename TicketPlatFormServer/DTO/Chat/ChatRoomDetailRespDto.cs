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
    public string? ThumbnailUrl { get; set; }
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
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
