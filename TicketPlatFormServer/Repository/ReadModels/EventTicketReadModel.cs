namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 이벤트 티켓 조회 ReadModel (Dapper 매핑용)
/// </summary>
public class EventTicketReadModel
{
    public int TicketId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public string? SeatGradeName { get; set; }
    public string? AreaName { get; set; }
    public string? Row { get; set; }
    public string? SeatInfo { get; set; }
    public int Quantity { get; set; }
    public int RemainingQuantity { get; set; }
    public int Price { get; set; }
    public int OriginalPrice { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public string? TransactionStatusCode { get; set; }
    public string? TransactionStatusName { get; set; }
    public long? TransactionId { get; set; }
    public string? SettlementStatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ThumbnailPath { get; set; }
}
