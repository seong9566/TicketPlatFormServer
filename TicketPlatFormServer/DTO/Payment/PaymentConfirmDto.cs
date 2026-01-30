using System.Text.Json.Serialization;

namespace TicketPlatFormServer.DTO.Payment;

/// <summary>
/// 결제 승인 요청 DTO (클라이언트 → 백엔드)
/// 토스페이먼츠 결제창에서 성공 후 전달되는 파라미터
/// </summary>
public class PaymentConfirmRequestDto
{
    /// <summary>
    /// 결제 키 (토스페이먼츠 제공)
    /// </summary>
    public string PaymentKey { get; set; } = null!;

    /// <summary>
    /// 주문 ID
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 결제 금액
    /// </summary>
    public int Amount { get; set; }
}

/// <summary>
/// 토스페이먼츠 결제 승인 API 요청 Body
/// </summary>
public class TossPaymentConfirmRequest
{
    [JsonPropertyName("paymentKey")]
    public string PaymentKey { get; set; } = null!;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = null!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}
