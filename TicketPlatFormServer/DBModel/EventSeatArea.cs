using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 구역
/// </summary>
public partial class EventSeatArea
{
    public int Id { get; set; }

    public int EventId { get; set; }

    /// <summary>
    /// 구역명 (F1, 1구역 등)
    /// </summary>
    public string AreaName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
