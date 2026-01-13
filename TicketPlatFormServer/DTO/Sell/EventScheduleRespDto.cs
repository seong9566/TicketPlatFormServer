namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 공연 일정 응답 DTO
/// </summary>
public class EventScheduleRespDto
{
    /// <summary>
    /// 일정 목록
    /// </summary>
    public List<ScheduleItem> Schedules { get; set; } = new();
}

/// <summary>
/// 일정 아이템
/// </summary>
public class ScheduleItem
{
    /// <summary>
    /// 일정 ID
    /// </summary>
    public string ScheduleId { get; set; } = null!;

    /// <summary>
    /// 공연 날짜 (YYYY-MM-DD)
    /// </summary>
    public string Date { get; set; } = null!;

    /// <summary>
    /// 공연 시간 (HH:mm)
    /// </summary>
    public string Time { get; set; } = null!;

    /// <summary>
    /// 요일 (예: 월, 화, 수)
    /// </summary>
    public string? DayOfWeek { get; set; }
}
