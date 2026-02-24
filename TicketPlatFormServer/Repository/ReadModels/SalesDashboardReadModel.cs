namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 판매 대시보드 조회 ReadModel (Dapper 매핑용)
/// </summary>
public class SalesDashboardReadModel
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public string? PosterImageUrl { get; set; }
    public string? VenueName { get; set; }
    public DateTime? EarliestEventDatetime { get; set; }
    public int TotalCount { get; set; }
    public int OnSaleCount { get; set; }
    public int CompletedCount { get; set; }
    public int SettlingCount { get; set; }
    public string? RepresentativeSeatInfo { get; set; }
}
