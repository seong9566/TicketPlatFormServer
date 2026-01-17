using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 등급 매핑 테이블
/// </summary>
public partial class EventSeatGrade
{
    public int Id { get; set; }

    /// <summary>
    /// 공연 FK
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 좌석 등급 FK
    /// </summary>
    public int SeatGradeId { get; set; }

    /// <summary>
    /// 활성화 여부
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual SeatGrade SeatGrade { get; set; } = null!;
}
