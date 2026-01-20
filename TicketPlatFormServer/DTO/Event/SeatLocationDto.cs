namespace TicketPlatFormServer.DTO;

/// <summary>
/// 좌석 위치 필터 정보 Dto
/// </summary>
public class SeatLocationDto
{
    /// <summary>
    /// 위치 ID
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// 위치명 (예: "1층", "2층", "플로어석")
    /// </summary>
    public string LocationName { get; set; } = null!;

    /// <summary>
    /// 해당 위치의 티켓 개수
    /// </summary>
    public int TicketCount { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }
}
