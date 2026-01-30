using System.Text.Json.Serialization;

namespace TicketPlatFormServer.DTO.Payment;

/// <summary>
/// 토스페이먼츠 결제 응답 DTO
/// 공식 문서: https://docs.tosspayments.com/reference#payment-객체
/// </summary>
public class TossPaymentResponseDto
{
    [JsonPropertyName("paymentKey")]
    public string PaymentKey { get; set; } = null!;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = null!;

    [JsonPropertyName("orderName")]
    public string OrderName { get; set; } = null!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!; // READY, IN_PROGRESS, WAITING_FOR_DEPOSIT, DONE, CANCELED, PARTIAL_CANCELED, ABORTED, EXPIRED

    [JsonPropertyName("requestedAt")]
    public string RequestedAt { get; set; } = null!;

    [JsonPropertyName("approvedAt")]
    public string? ApprovedAt { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = null!; // 카드, 가상계좌, 계좌이체, 간편결제 등

    [JsonPropertyName("totalAmount")]
    public int TotalAmount { get; set; }

    [JsonPropertyName("balanceAmount")]
    public int BalanceAmount { get; set; }

    [JsonPropertyName("suppliedAmount")]
    public int SuppliedAmount { get; set; }

    [JsonPropertyName("vat")]
    public int Vat { get; set; }

    [JsonPropertyName("taxFreeAmount")]
    public int TaxFreeAmount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "KRW";

    [JsonPropertyName("receipt")]
    public TossReceiptDto? Receipt { get; set; }

    [JsonPropertyName("card")]
    public TossCardDto? Card { get; set; }

    [JsonPropertyName("virtualAccount")]
    public TossVirtualAccountDto? VirtualAccount { get; set; }

    [JsonPropertyName("transfer")]
    public TossTransferDto? Transfer { get; set; }

    [JsonPropertyName("easyPay")]
    public TossEasyPayDto? EasyPay { get; set; }

    [JsonPropertyName("cancels")]
    public List<TossCancelDto>? Cancels { get; set; }

    [JsonPropertyName("failure")]
    public TossFailureDto? Failure { get; set; }

    // 추가 필드 - 토스페이먼츠 API 응답 완전 대응
    [JsonPropertyName("mId")]
    public string? MId { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("lastTransactionKey")]
    public string? LastTransactionKey { get; set; }

    [JsonPropertyName("useEscrow")]
    public bool UseEscrow { get; set; }

    [JsonPropertyName("cultureExpense")]
    public bool CultureExpense { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; } // NORMAL, BILLING

    [JsonPropertyName("country")]
    public string? Country { get; set; } // KR

    [JsonPropertyName("isPartialCancelable")]
    public bool IsPartialCancelable { get; set; }

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    [JsonPropertyName("discount")]
    public object? Discount { get; set; }

    [JsonPropertyName("checkout")]
    public TossCheckoutDto? Checkout { get; set; }

    [JsonPropertyName("mobilePhone")]
    public TossMobilePhoneDto? MobilePhone { get; set; }

    [JsonPropertyName("giftCertificate")]
    public TossGiftCertificateDto? GiftCertificate { get; set; }

    [JsonPropertyName("cashReceipt")]
    public TossCashReceiptDto? CashReceipt { get; set; }

    [JsonPropertyName("cashReceipts")]
    public object? CashReceipts { get; set; }
}

/// <summary>
/// 영수증 정보
/// </summary>
public class TossReceiptDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}

/// <summary>
/// 카드 결제 정보
/// </summary>
public class TossCardDto
{
    [JsonPropertyName("company")]
    public string Company { get; set; } = null!;

    [JsonPropertyName("number")]
    public string Number { get; set; } = null!;

    [JsonPropertyName("installmentPlanMonths")]
    public int InstallmentPlanMonths { get; set; }

    [JsonPropertyName("approveNo")]
    public string ApproveNo { get; set; } = null!;

    [JsonPropertyName("cardType")]
    public string CardType { get; set; } = null!; // 신용, 체크 등

    [JsonPropertyName("ownerType")]
    public string OwnerType { get; set; } = null!; // 개인, 법인

    [JsonPropertyName("acquireStatus")]
    public string AcquireStatus { get; set; } = null!;

    [JsonPropertyName("isInterestFree")]
    public bool IsInterestFree { get; set; }

    [JsonPropertyName("issuerCode")]
    public string? IssuerCode { get; set; } // 카드 발급사 코드

    [JsonPropertyName("acquirerCode")]
    public string? AcquirerCode { get; set; } // 카드 매입사 코드

    [JsonPropertyName("interestPayer")]
    public string? InterestPayer { get; set; } // 무이자 할부 부담자

    [JsonPropertyName("useCardPoint")]
    public bool UseCardPoint { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; } // 카드 결제 금액
}

/// <summary>
/// 가상계좌 정보
/// </summary>
public class TossVirtualAccountDto
{
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = null!;

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = null!;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = null!;

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = null!;

    [JsonPropertyName("refundStatus")]
    public string? RefundStatus { get; set; }

    [JsonPropertyName("expired")]
    public bool Expired { get; set; }

    [JsonPropertyName("settlementStatus")]
    public string? SettlementStatus { get; set; }

    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; } // 일반, 고정

    [JsonPropertyName("refundReceiveAccount")]
    public object? RefundReceiveAccount { get; set; }
}

/// <summary>
/// 계좌이체 정보
/// </summary>
public class TossTransferDto
{
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = null!;

    [JsonPropertyName("settlementStatus")]
    public string SettlementStatus { get; set; } = null!;
}

/// <summary>
/// 간편결제 정보
/// </summary>
public class TossEasyPayDto
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = null!; // 토스페이, 네이버페이, 카카오페이 등

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("discountAmount")]
    public int DiscountAmount { get; set; }
}

/// <summary>
/// 취소 내역
/// </summary>
public class TossCancelDto
{
    [JsonPropertyName("cancelAmount")]
    public int CancelAmount { get; set; }

    [JsonPropertyName("cancelReason")]
    public string CancelReason { get; set; } = null!;

    [JsonPropertyName("taxFreeAmount")]
    public int TaxFreeAmount { get; set; }

    [JsonPropertyName("taxAmount")]
    public int TaxAmount { get; set; }

    [JsonPropertyName("refundableAmount")]
    public int RefundableAmount { get; set; }

    [JsonPropertyName("canceledAt")]
    public string CanceledAt { get; set; } = null!;
}

/// <summary>
/// 결제 실패 정보
/// </summary>
public class TossFailureDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}

/// <summary>
/// 결제창 정보
/// </summary>
public class TossCheckoutDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}

/// <summary>
/// 휴대폰 결제 정보
/// </summary>
public class TossMobilePhoneDto
{
    [JsonPropertyName("customerMobilePhone")]
    public string CustomerMobilePhone { get; set; } = null!;

    [JsonPropertyName("settlementStatus")]
    public string SettlementStatus { get; set; } = null!; // INCOMPLETED, COMPLETED

    [JsonPropertyName("receiptUrl")]
    public string ReceiptUrl { get; set; } = null!;
}

/// <summary>
/// 상품권 결제 정보
/// </summary>
public class TossGiftCertificateDto
{
    [JsonPropertyName("approveNo")]
    public string ApproveNo { get; set; } = null!;

    [JsonPropertyName("settlementStatus")]
    public string SettlementStatus { get; set; } = null!; // INCOMPLETED, COMPLETED
}

/// <summary>
/// 현금영수증 정보
/// </summary>
public class TossCashReceiptDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!; // 소득공제, 지출증빙

    [JsonPropertyName("receiptKey")]
    public string ReceiptKey { get; set; } = null!;

    [JsonPropertyName("issueNumber")]
    public string IssueNumber { get; set; } = null!;

    [JsonPropertyName("receiptUrl")]
    public string ReceiptUrl { get; set; } = null!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("taxFreeAmount")]
    public int TaxFreeAmount { get; set; }
}
