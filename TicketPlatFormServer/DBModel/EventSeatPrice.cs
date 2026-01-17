using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 공연별 좌석 정가 테이블
/// </summary>
public partial class EventSeatPrice
{
    public int Id { get; set; }

    /// <summary>
    /// 공연 FK
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 좌석 등급 FK
    /// </summary>
    public int SeatGradeId { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 활성화 여부
    /// </summary>
    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual SeatGrade SeatGrade { get; set; } = null!;
}
