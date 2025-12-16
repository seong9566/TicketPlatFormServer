using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 거래 상태 변경 이력 테이블
/// </summary>
public partial class TransactionHistory
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    /// <summary>
    /// 이전 상태 코드
    /// </summary>
    public string? OldStatus { get; set; }

    /// <summary>
    /// 새 상태 코드
    /// </summary>
    public string? NewStatus { get; set; }

    /// <summary>
    /// 변경자 FK
    /// </summary>
    public long? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
