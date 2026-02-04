using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 결제 거래 히스토리 (승인/취소/부분취소 모든 이벤트 추적)
/// </summary>
public partial class PaymentTransaction
{
    public ulong Id { get; set; }

    /// <summary>
    /// payments FK
    /// </summary>
    public ulong PaymentId { get; set; }

    /// <summary>
    /// 거래 키 (토스페이먼츠 제공)
    /// </summary>
    public string TransactionKey { get; set; } = null!;

    /// <summary>
    /// 거래 유형 (PAYMENT, CANCEL, PARTIAL_CANCEL)
    /// </summary>
    public string TransactionType { get; set; } = null!;

    /// <summary>
    /// 거래 금액
    /// </summary>
    public ulong Amount { get; set; }

    /// <summary>
    /// 잔액 (부분 취소 후 잔여 금액)
    /// </summary>
    public ulong? BalanceAmount { get; set; }

    /// <summary>
    /// 비과세 금액
    /// </summary>
    public ulong TaxFreeAmount { get; set; }

    /// <summary>
    /// 통화 코드 (ISO-4217)
    /// </summary>
    public string Currency { get; set; } = null!;

    /// <summary>
    /// 거래 상태 (DONE, FAILED, PENDING)
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 거래 사유 (취소 시 필수)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 토스 API 전체 응답 (암호화 필수, Base64 인코딩된 암호문)
    /// </summary>
    public string? TossResponse { get; set; }

    /// <summary>
    /// 토스 이벤트 발생 시각 (API 제공)
    /// </summary>
    public DateTime? EventAt { get; set; }

    /// <summary>
    /// 저장 시각 (UTC)
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
