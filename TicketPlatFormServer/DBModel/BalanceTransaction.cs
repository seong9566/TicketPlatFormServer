using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 잔고 거래 내역 테이블
/// </summary>
public partial class BalanceTransaction
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public long Amount { get; set; }

    public long BalanceAfter { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
