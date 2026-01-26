using System.Text.Json.Serialization;

namespace TicketPlatFormServer.DTO.Payment;

/// <summary>
/// 결제 취소(환불) 요청 DTO (클라이언트 → 백엔드)
/// </summary>
public class PaymentCancelRequestDto
{
    /// <summary>
    /// 거래 ID
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 취소 사유
    /// </summary>
    public string CancelReason { get; set; } = null!;

    /// <summary>
    /// 부분 취소 금액 (null이면 전액 취소)
    /// </summary>
    public int? CancelAmount { get; set; }
}

/// <summary>
/// 토스페이먼츠 결제 취소 API 요청 Body
/// </summary>
public class TossPaymentCancelRequest
{
    [JsonPropertyName("cancelReason")]
    public string CancelReason { get; set; } = null!;

    [JsonPropertyName("cancelAmount")]
    public int? CancelAmount { get; set; }

    [JsonPropertyName("refundableAmount")]
    public int? RefundableAmount { get; set; }

    [JsonPropertyName("taxFreeAmount")]
    public int? TaxFreeAmount { get; set; }
}

/// <summary>
/// 결제 취소 응답 DTO
/// </summary>
public class PaymentCancelResponseDto
{
    public string PaymentKey { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int CancelAmount { get; set; }
    public string CancelReason { get; set; } = null!;
    public DateTime CanceledAt { get; set; }
}
