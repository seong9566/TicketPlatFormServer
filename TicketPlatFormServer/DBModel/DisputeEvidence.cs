using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 분쟁 증거 자료 테이블
/// </summary>
public partial class DisputeEvidence
{
    public long Id { get; set; }

    /// <summary>
    /// 분쟁 FK
    /// </summary>
    public long DisputeId { get; set; }

    /// <summary>
    /// 증거 이미지 URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 설명
    /// </summary>
    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Dispute Dispute { get; set; } = null!;
}
