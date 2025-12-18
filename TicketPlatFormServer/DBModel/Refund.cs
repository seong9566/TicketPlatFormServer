using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 환불 정보 테이블
/// </summary>
public partial class Refund
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    /// <summary>
    /// 결제 FK
    /// </summary>
    public long PaymentId { get; set; }

    /// <summary>
    /// 환불 금액
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 환불 사유 FK
    /// </summary>
    public long ReasonId { get; set; }

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// 요청자 FK
    /// </summary>
    public long RequestedBy { get; set; }

    /// <summary>
    /// 승인자 FK
    /// </summary>
    public long? ApprovedBy { get; set; }

    /// <summary>
    /// 처리 완료 시각
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual RefundReason Reason { get; set; } = null!;

    public virtual RefundStatus Status { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
