namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 좌석 등급 조회 ReadModel (Dapper 매핑용)
/// </summary>
public class SeatGradeReadModel
{
    public int GradeId { get; set; }
    public int EventId { get; set; }
    public int SeatGradeId { get; set; }
    public string Code { get; set; } = null!;
    public string NameKo { get; set; } = null!;
    public string? NameEn { get; set; }
    public int? OriginalPrice { get; set; }
    public int SortOrder { get; set; }
}
