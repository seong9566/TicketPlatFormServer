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
    /// 공연 일시
    /// </summary>
    public DateTime EventDatetime { get; set; }

    /// <summary>
    /// 좌석 등급 FK
    /// </summary>
    public int? SeatGradeId { get; set; }

    /// <summary>
    /// 좌석 위치 FK (event_seat_locations 테이블)
    /// </summary>
    public int? SeatLocationId { get; set; }

    /// <summary>
    /// 좌석 구역 FK (seat_areas 테이블)
    /// </summary>
    public int? AreaId { get; set; }

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
    /// 거래 방법 FK
    /// </summary>
    public int? TradeMethodId { get; set; }



    /// <summary>
    /// 티켓 보유 여부 (1=보유, 0=미보유)
    /// </summary>
    public bool? HasTicket { get; set; }

    public virtual TicketCategory Category { get; set; } = null!;

    public virtual Event? Event { get; set; }

    public virtual EventSchedule? Schedule { get; set; }

    /// <summary>
    /// 좌석 위치 Navigation 속성 (event_seat_locations)
    /// </summary>
    public virtual EventSeatLocation? SeatLocation { get; set; }

    public virtual TicketStatus Status { get; set; } = null!;

    public virtual SeatGrade? SeatGrade { get; set; }

    public virtual TradeMethod? TradeMethod { get; set; }

    /// <summary>
    /// 좌석 구역 Navigation 속성
    /// </summary>
    public virtual SeatArea? SeatArea { get; set; }

    public virtual ICollection<TicketTicketFeature> TicketTicketFeatures { get; set; } = new List<TicketTicketFeature>();
}
