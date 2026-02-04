using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 현금영수증 정보 (1:N 관계 허용)
/// </summary>
public partial class PaymentCashReceipt
{
    public ulong Id { get; set; }

    /// <summary>
    /// payments FK
    /// </summary>
    public ulong PaymentId { get; set; }

    /// <summary>
    /// 소득공제/지출증빙
    /// </summary>
    public string ReceiptType { get; set; } = null!;

    /// <summary>
    /// 현금영수증 키
    /// </summary>
    public string ReceiptKey { get; set; } = null!;

    /// <summary>
    /// 발급 번호
    /// </summary>
    public string IssueNumber { get; set; } = null!;

    /// <summary>
    /// 현금영수증 URL
    /// </summary>
    public string ReceiptUrl { get; set; } = null!;

    /// <summary>
    /// 현금영수증 금액
    /// </summary>
    public ulong Amount { get; set; }

    /// <summary>
    /// 비과세 금액
    /// </summary>
    public ulong TaxFreeAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
