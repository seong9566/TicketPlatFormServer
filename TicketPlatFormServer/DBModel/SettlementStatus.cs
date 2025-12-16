using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 정산 상태 코드 테이블
/// </summary>
public partial class SettlementStatus
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
}
