namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 내 판매 티켓 조회 ReadModel (Dapper 매핑용)
/// </summary>
public class MyTicketReadModel
{
    public int TicketId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public int? SeatGradeId { get; set; }
    public string? SeatGradeName { get; set; }
    public string? AreaName { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    public int RemainingQuantity { get; set; }
    public int StatusId { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
