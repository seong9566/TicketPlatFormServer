using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 특이사항 마스터
/// </summary>
public partial class TicketFeature
{
    public int Id { get; set; }

    /// <summary>
    /// 특이사항 코드
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 한글명
    /// </summary>
    public string NameKo { get; set; } = null!;

    /// <summary>
    /// 영문명
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// 설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 아이콘 (UI용)
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
