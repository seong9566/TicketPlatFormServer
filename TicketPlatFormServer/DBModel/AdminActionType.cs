using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 관리자 액션 유형 코드 테이블
/// </summary>
public partial class AdminActionType
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    /// <summary>
    /// 한글 표시명
    /// </summary>
    public string? NameKo { get; set; }

    /// <summary>
    /// 활성화 여부
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }

    public virtual ICollection<AdminAction> AdminActions { get; set; } = new List<AdminAction>();
}
