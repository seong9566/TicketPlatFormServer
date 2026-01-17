using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 구역 테이블
/// </summary>
public partial class SeatArea
{
    public int Id { get; set; }

    /// <summary>
    /// 공연 FK
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 구역명 (F1, 1구역 등)
    /// </summary>
    public string AreaName { get; set; } = null!;

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
