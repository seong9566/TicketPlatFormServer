using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 간편결제 상세 정보
/// </summary>
public partial class PaymentEasyPayDetail
{
    public ulong Id { get; set; }

    /// <summary>
    /// payments FK
    /// </summary>
    public long PaymentId { get; set; }

    /// <summary>
    /// 간편결제 제공자 (토스페이/카카오페이/네이버페이)
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 간편결제 금액
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// 할인 금액
    /// </summary>
    public long DiscountAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Property
    public virtual Payment Payment { get; set; } = null!;
}
