using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 관리자 액션 로그 테이블
/// </summary>
public partial class AdminAction
{
    public long Id { get; set; }

    /// <summary>
    /// 관리자 FK
    /// </summary>
    public long AdminId { get; set; }

    /// <summary>
    /// 액션 유형 FK
    /// </summary>
    public long ActionTypeId { get; set; }

    /// <summary>
    /// 대상 유형 FK
    /// </summary>
    public long TargetTypeId { get; set; }

    /// <summary>
    /// 대상 ID
    /// </summary>
    public long TargetId { get; set; }

    /// <summary>
    /// 사유
    /// </summary>
    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AdminActionType ActionType { get; set; } = null!;

    public virtual User Admin { get; set; } = null!;

    public virtual AdminTargetType TargetType { get; set; } = null!;
}
