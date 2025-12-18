using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 분쟁 테이블
/// </summary>
public partial class Dispute
{
    public long Id { get; set; }

    /// <summary>
    /// 거래 FK
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 신고자 FK
    /// </summary>
    public long ClaimantId { get; set; }

    /// <summary>
    /// 분쟁 유형 FK
    /// </summary>
    public long TypeId { get; set; }

    /// <summary>
    /// 분쟁 내용
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<DisputeEvidence> DisputeEvidences { get; set; } = new List<DisputeEvidence>();

    public virtual DisputeStatus Status { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual DisputeType Type { get; set; } = null!;
}
