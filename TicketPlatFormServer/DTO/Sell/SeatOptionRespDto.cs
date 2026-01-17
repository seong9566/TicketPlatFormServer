namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 좌석 옵션 응답 DTO
/// </summary>
public class SeatOptionRespDto
{
    /// <summary>
    /// 좌석 위치 옵션 목록
    /// </summary>
    public List<SeatLocationOption> Locations { get; set; } = new();

    /// <summary>
    /// 직접 입력 허용 여부
    /// </summary>
    public bool AllowCustomLocation { get; set; } = true;
}

/// <summary>
/// 좌석 위치 옵션
/// </summary>
public class SeatLocationOption
{
    /// <summary>
    /// 위치 ID
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// 위치명
    /// </summary>
    public string LocationName { get; set; } = null!;
}
