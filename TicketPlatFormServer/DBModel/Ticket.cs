using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 정보 테이블
/// </summary>
public partial class Ticket
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    /// <summary>
    /// 공연 FK
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    /// 일정 FK
    /// </summary>
    public string? ScheduleId { get; set; }

    public int CategoryId { get; set; }

    /// <summary>
    /// 티켓 제목
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 공연 일시
    /// </summary>
    public DateTime EventDatetime { get; set; }

    /// <summary>
    /// 좌석 정보
    /// </summary>
    public string? SeatInfo { get; set; }

    /// <summary>
    /// 좌석 위치 FK
    /// </summary>
    public string? LocationId { get; set; }

    /// <summary>
    /// 구역 (예: A구역)
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// 열 (예: 5열)
    /// </summary>
    public string? Row { get; set; }

    /// <summary>
    /// 총 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 연석 여부
    /// </summary>
    public bool? IsConsecutive { get; set; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// 판매가
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 정가
    /// </summary>
    public int OriginalPrice { get; set; }

    /// <summary>
    /// 상세 설명
    /// </summary>
    public string? Description { get; set; }

    public int StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft Delete 시각
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 좌석 특징 키워드 (JSON 배열)
    /// </summary>
    public string? SeatFeatures { get; set; }

    public virtual TicketCategory Category { get; set; } = null!;

    public virtual Event? Event { get; set; }

    public virtual EventSchedule? Schedule { get; set; }

    public virtual SeatLocation? Location { get; set; }

    public virtual TicketStatus Status { get; set; } = null!;
}
