using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 거래 정보 테이블 (하나의 거래에 여러 티켓 항목 가능)
/// </summary>
public partial class Transaction
{
    public long Id { get; set; }

    /// <summary>
    /// 구매자 FK
    /// </summary>
    public long BuyerId { get; set; }

    /// <summary>
    /// 판매자 FK
    /// </summary>
    public long SellerId { get; set; }

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// 예약 시각
    /// </summary>
    public DateTime? ReservedAt { get; set; }

    /// <summary>
    /// 예약 만료 시각
    /// </summary>
    public DateTime? ReservationExpiresAt { get; set; }

    /// <summary>
    /// 구매 확정 시각
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// 자동 확정 예정 시각
    /// </summary>
    public DateTime? AutoConfirmAt { get; set; }

    /// <summary>
    /// 확정자 유형 FK
    /// </summary>
    public long? ConfirmedById { get; set; }

    /// <summary>
    /// 취소 시각
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Soft Delete 시각
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 총 거래 금액 (TransactionItem의 TotalPrice 합계)
    /// </summary>
    public int? Amount { get; set; }

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual TransactionConfirmedBy? ConfirmedBy { get; set; }

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual Escrow? Escrow { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual TransactionStatus Status { get; set; } = null!;

    public virtual ICollection<TicketVerification> TicketVerifications { get; set; } = new List<TicketVerification>();

    public virtual ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();

    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();

    public virtual ICollection<UserReputation> UserReputations { get; set; } = new List<UserReputation>();
}
