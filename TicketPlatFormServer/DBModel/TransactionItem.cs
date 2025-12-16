using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 거래 항목 테이블 (티켓별 구매 정보)
/// </summary>
public partial class TransactionItem
{
    public long Id { get; set; }

    /// <summary>
    /// 거래 FK
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 티켓 FK
    /// </summary>
    public long TicketId { get; set; }

    /// <summary>
    /// 구매 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 단가
    /// </summary>
    public int UnitPrice { get; set; }

    /// <summary>
    /// 소계 (단가 × 수량)
    /// </summary>
    public int TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
