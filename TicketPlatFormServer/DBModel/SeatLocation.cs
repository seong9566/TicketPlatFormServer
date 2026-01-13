using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 좌석 위치 옵션 테이블
/// </summary>
public partial class SeatLocation
{
    /// <summary>
    /// 위치 ID (예: LOC_1F)
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 공연 FK (NULL이면 전역 사용)
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    /// 위치명
    /// </summary>
    public string LocationName { get; set; } = null!;

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual Event? Event { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
