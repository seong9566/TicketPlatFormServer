using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 거래 방식 마스터
/// </summary>
public partial class TradeMethod
{
    public int Id { get; set; }

    /// <summary>
    /// 거래 방식 코드
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
    /// 정렬 순서
    /// </summary>
    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
