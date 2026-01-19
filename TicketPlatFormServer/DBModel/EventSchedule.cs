using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연 일정 테이블
/// </summary>
public partial class EventSchedule
{
    /// <summary>
    /// 일정 ID (예: sch001)
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 공연 FK
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 공연 날짜
    /// </summary>
    public DateOnly ScheduleDate { get; set; }

    /// <summary>
    /// 공연 시간
    /// </summary>
    public TimeOnly ScheduleTime { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;
}
