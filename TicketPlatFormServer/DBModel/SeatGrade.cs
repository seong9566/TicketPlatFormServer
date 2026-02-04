using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 좌석 등급 마스터
/// </summary>
public partial class SeatGrade
{
    public int Id { get; set; }

    /// <summary>
    /// 좌석 등급 코드
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
    /// 정렬 순서
    /// </summary>
    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<EventSeatGrade> EventSeatGrades { get; set; } = new List<EventSeatGrade>();
}
