using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 등급 매핑
/// </summary>
public partial class EventSeatGrade
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int SeatGradeId { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual SeatGrade SeatGrade { get; set; } = null!;
}
