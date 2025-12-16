using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 분쟁 유형 코드 테이블
/// </summary>
public partial class DisputeType
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
}
