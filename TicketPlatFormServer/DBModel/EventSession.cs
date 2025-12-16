using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 이벤트 회차/세션 정보 테이블
/// </summary>
public partial class EventSession
{
    public long Id { get; set; }

    /// <summary>
    /// 이벤트 FK
    /// </summary>
    public long EventId { get; set; }

    /// <summary>
    /// 시작 일시
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// 종료 일시
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 공연장 이름
    /// </summary>
    public string? VenueName { get; set; }

    /// <summary>
    /// 공연장 주소
    /// </summary>
    public string? VenueAddress { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
