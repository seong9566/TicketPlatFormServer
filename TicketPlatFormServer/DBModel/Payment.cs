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
    /// 토스 가맹점 ID (mId)
    /// </summary>
    public string? MerchantId { get; set; }

    /// <summary>
    /// 토스 API 버전
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// 국가 코드 (ISO-3166-1 alpha-2)
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// PG사 결제 키
    /// </summary>
    public string? PaymentKey { get; set; }

    /// <summary>
    /// 주문 ID
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// 결제 금액 (KRW, 원 단위)
    /// </summary>
    public long Amount { get; set; }

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

    // 토스페이먼츠 추가 필드
    /// <summary>
    /// 에스크로 사용 여부
    /// </summary>
    public bool UseEscrow { get; set; }

    /// <summary>
    /// 부분 취소 가능 여부
    /// </summary>
    public bool IsPartialCancelable { get; set; }

    /// <summary>
    /// 결제 타입 (NORMAL, BILLING)
    /// </summary>
    public string? PaymentType { get; set; }

    /// <summary>
    /// 최종 거래 키 (deprecated: use PaymentTransactions)
    /// </summary>
    public string? LastTransactionKey { get; set; }

    /// <summary>
    /// 문화비 소득공제 여부
    /// </summary>
    public bool CultureExpense { get; set; }

    /// <summary>
    /// 커스텀 메타데이터 (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// 할인 정보 (JSON)
    /// </summary>
    public string? DiscountInfo { get; set; }

    // Navigation Properties
    public virtual PaymentMethod Method { get; set; } = null!;

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual PaymentStatus Status { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    // 결제 수단별 상세 정보
    public virtual PaymentCardDetail? CardDetail { get; set; }

    public virtual PaymentVirtualAccountDetail? VirtualAccountDetail { get; set; }

    public virtual PaymentEasyPayDetail? EasyPayDetail { get; set; }

    public virtual ICollection<PaymentCashReceipt> CashReceipts { get; set; } = new List<PaymentCashReceipt>();

    public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}
