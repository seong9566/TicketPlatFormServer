using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 등급 매핑
/// </summary>
public partial class EventSeatGrade
{
    public int Id { get; set; }

    public int EventId { get; set; }

    /// <summary>
    /// 기존 SeatGrade 참조용 (마이그레이션 목적, 나중에 삭제 가능)
    /// </summary>
    public int? SeatGradeId { get; set; }

    /// <summary>
    /// 등급 코드 (VIP, R, S 등)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 한글 명칭
    /// </summary>
    public string NameKo { get; set; } = null!;

    /// <summary>
    /// 영문 명칭
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// 해당 공연의 해당 등급 정가
    /// </summary>
    public int? OriginalPrice { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
