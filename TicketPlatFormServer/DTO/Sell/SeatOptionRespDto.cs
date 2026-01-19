namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 좌석 옵션 응답 DTO
/// </summary>
public class SeatOptionRespDto
{
    /// <summary>
    /// 좌석 등급 옵션 목록 (VIP, 일반, 지정석 등)
    /// </summary>
    public List<SeatGradeOption> Grades { get; set; } = new();

    /// <summary>
    /// 좌석 위치 옵션 목록 (1층, 2층, 플로어석 등)
    /// </summary>
    public List<SeatLocationOption> Locations { get; set; } = new();

    /// <summary>
    /// 좌석 구역 옵션 목록 (F1, A구역 등)
    /// </summary>
    public List<SeatAreaOption> Areas { get; set; } = new();

    /// <summary>
    /// 직접 입력 허용 여부
    /// </summary>
    public bool AllowCustomLocation { get; set; } = true;
}

/// <summary>
/// 좌석 등급 옵션
/// </summary>
public class SeatGradeOption
{
    /// <summary>
    /// 등급 ID
    /// </summary>
    public int GradeId { get; set; }

    /// <summary>
    /// 등급 코드
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 등급명
    /// </summary>
    public string GradeName { get; set; } = null!;
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

/// <summary>
/// 좌석 구역 옵션
/// </summary>
public class SeatAreaOption
{
    /// <summary>
    /// 구역 ID
    /// </summary>
    public int AreaId { get; set; }

    /// <summary>
    /// 구역명
    /// </summary>
    public string AreaName { get; set; } = null!;
}
