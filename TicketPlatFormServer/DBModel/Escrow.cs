using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 에스크로 (결제 대금 보관) 테이블
/// </summary>
public partial class Escrow
{
    public long Id { get; set; }

    /// <summary>
    /// 거래 FK (1:1)
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 총 금액
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 수수료
    /// </summary>
    public int FeeAmount { get; set; }

    /// <summary>
    /// 판매자 정산 금액
    /// </summary>
    public int SellerAmount { get; set; }

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 정산 완료 시각
    /// </summary>
    public DateTime? ReleasedAt { get; set; }

    /// <summary>
    /// 환불 완료 시각
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual EscrowStatus Status { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
