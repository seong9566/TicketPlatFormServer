using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 가격 변경 이력 테이블
/// </summary>
public partial class TicketPriceHistory
{
    public long Id { get; set; }

    public long TicketId { get; set; }

    /// <summary>
    /// 변경 전 가격
    /// </summary>
    public int OldPrice { get; set; }

    /// <summary>
    /// 변경 후 가격
    /// </summary>
    public int NewPrice { get; set; }

    /// <summary>
    /// 변경 사유
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 변경자 FK
    /// </summary>
    public long? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
