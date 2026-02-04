using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 카드 결제 상세 정보 (PCI DSS 주의: 마스킹된 정보만 저장)
/// </summary>
public partial class PaymentCardDetail
{
    public ulong Id { get; set; }

    /// <summary>
    /// payments FK
    /// </summary>
    public ulong PaymentId { get; set; }

    /// <summary>
    /// 카드사명
    /// </summary>
    public string Company { get; set; } = null!;

    /// <summary>
    /// 마스킹된 카드번호 (PCI DSS 준수)
    /// </summary>
    public string CardNumber { get; set; } = null!;

    /// <summary>
    /// 할부 개월 수
    /// </summary>
    public int InstallmentPlanMonths { get; set; }

    /// <summary>
    /// 승인번호
    /// </summary>
    public string ApproveNo { get; set; } = null!;

    /// <summary>
    /// 신용/체크
    /// </summary>
    public string CardType { get; set; } = null!;

    /// <summary>
    /// 개인/법인
    /// </summary>
    public string OwnerType { get; set; } = null!;

    /// <summary>
    /// 매입 상태
    /// </summary>
    public string AcquireStatus { get; set; } = null!;

    /// <summary>
    /// 무이자 여부
    /// </summary>
    public bool IsInterestFree { get; set; }

    /// <summary>
    /// 카드 발급사 코드
    /// </summary>
    public string? IssuerCode { get; set; }

    /// <summary>
    /// 카드 매입사 코드
    /// </summary>
    public string? AcquirerCode { get; set; }

    /// <summary>
    /// 무이자 할부 부담자 (BUYER/CARD_COMPANY/MERCHANT)
    /// </summary>
    public string? InterestPayer { get; set; }

    /// <summary>
    /// 카드 포인트 사용 여부
    /// </summary>
    public bool UseCardPoint { get; set; }

    /// <summary>
    /// 카드 결제 금액
    /// </summary>
    public ulong Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
