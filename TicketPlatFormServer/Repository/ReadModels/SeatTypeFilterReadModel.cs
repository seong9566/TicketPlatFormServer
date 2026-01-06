namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 좌석 타입 필터 정보 ReadModel (Repository 반환용)
/// </summary>
public class SeatTypeFilterReadModel
{
    /// <summary>
    /// 좌석 타입명 (예: "전체좌석", "VIP석", "R석", "S석" 등)
    /// </summary>
    public string SeatTypeName { get; set; } = null!;

    /// <summary>
    /// 해당 좌석 타입의 티켓 개수
    /// </summary>
    public int TicketCount { get; set; }
}
