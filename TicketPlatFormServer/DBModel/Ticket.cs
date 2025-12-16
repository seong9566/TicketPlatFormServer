using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 정보 테이블
/// </summary>
public partial class Ticket
{
    public long Id { get; set; }

    /// <summary>
    /// 판매자 FK
    /// </summary>
    public long SellerId { get; set; }

    /// <summary>
    /// 이벤트 세션 FK
    /// </summary>
    public long? EventSessionId { get; set; }

    /// <summary>
    /// 카테고리 FK
    /// </summary>
    public long CategoryId { get; set; }

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
    /// 총 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// 연석 여부
    /// </summary>
    public bool? IsContinuous { get; set; }

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

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft Delete 시각
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual TicketCategory Category { get; set; } = null!;

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual EventSession? EventSession { get; set; }

    public virtual User Seller { get; set; } = null!;

    public virtual TicketStatus Status { get; set; } = null!;

    public virtual ICollection<TicketImage> TicketImages { get; set; } = new List<TicketImage>();

    public virtual ICollection<TicketPriceHistory> TicketPriceHistories { get; set; } = new List<TicketPriceHistory>();

    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}
