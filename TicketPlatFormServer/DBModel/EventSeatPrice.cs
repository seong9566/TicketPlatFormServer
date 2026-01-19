using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 정가
/// </summary>
public partial class EventSeatPrice
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int SeatGradeId { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual SeatGrade SeatGrade { get; set; } = null!;
}
