using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 위치 테이블
/// </summary>
public partial class EventSeatLocation
{
    public int Id { get; set; }

    /// <summary>
    /// 공연 FK
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 위치명 (플로어석, 1층 등)
    /// </summary>
    public string LocationName { get; set; } = null!;

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

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
