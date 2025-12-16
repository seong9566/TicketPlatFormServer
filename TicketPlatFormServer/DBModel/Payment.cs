using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 결제 정보 테이블
/// </summary>
public partial class Payment
{
    public long Id { get; set; }

    /// <summary>
    /// 거래 FK
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// PG사 (예: toss, kakao)
    /// </summary>
    public string? PgProvider { get; set; }

    /// <summary>
    /// PG사 결제 키
    /// </summary>
    public string? PaymentKey { get; set; }

    /// <summary>
    /// 주문 ID
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// 결제 금액
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 결제 수단 FK
    /// </summary>
    public long MethodId { get; set; }

    /// <summary>
    /// 결제 완료 시각
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// 결제 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    public virtual PaymentMethod Method { get; set; } = null!;

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual PaymentStatus Status { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
