using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 관리자 작업 대상 유형 코드 테이블
/// </summary>
public partial class AdminTargetType
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    /// <summary>
    /// 한글 표시명
    /// </summary>
    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<AdminAction> AdminActions { get; set; } = new List<AdminAction>();
}
