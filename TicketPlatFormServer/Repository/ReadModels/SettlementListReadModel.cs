namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 정산 목록 조회 ReadModel (Dapper 매핑용)
/// </summary>
public class SettlementListReadModel
{
    public long Id { get; set; }
    public long TransactionId { get; set; }
    public int Amount { get; set; }
    public int Fee { get; set; }
    public int NetAmount { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? FailureReason { get; set; }
    public int? RetryCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string EventTitle { get; set; } = null!;
    public string? SeatInfo { get; set; }
}
