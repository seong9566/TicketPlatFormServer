using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 가상계좌 결제 상세 정보 (민감정보 암호화 필수)
/// </summary>
public partial class PaymentVirtualAccountDetail
{
    public ulong Id { get; set; }

    /// <summary>
    /// payments FK
    /// </summary>
    public ulong PaymentId { get; set; }

    /// <summary>
    /// 가상계좌 번호 (민감정보: 암호화 권장)
    /// </summary>
    public string AccountNumber { get; set; } = null!;

    /// <summary>
    /// 은행 코드
    /// </summary>
    public string BankCode { get; set; } = null!;

    /// <summary>
    /// 입금자명 (PII: 암호화 권장)
    /// </summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>
    /// 입금 기한
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// 환불 상태
    /// </summary>
    public string? RefundStatus { get; set; }

    /// <summary>
    /// 만료 여부
    /// </summary>
    public bool Expired { get; set; }

    /// <summary>
    /// 정산 상태
    /// </summary>
    public string? SettlementStatus { get; set; }

    /// <summary>
    /// 계좌 유형 (일반/고정)
    /// </summary>
    public string? AccountType { get; set; }

    /// <summary>
    /// 환불 받을 계좌 정보 (암호화 필수, Base64 인코딩된 암호문)
    /// </summary>
    public string? RefundReceiveAccount { get; set; }

    /// <summary>
    /// 가상계좌 시크릿 (민감정보: 암호화 필수)
    /// </summary>
    public string? Secret { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
