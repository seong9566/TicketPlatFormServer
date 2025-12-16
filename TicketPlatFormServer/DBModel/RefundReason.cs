using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 환불 사유 코드 테이블
/// </summary>
public partial class RefundReason
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
