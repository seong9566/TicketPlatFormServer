using System.Text.Json.Serialization;

namespace TicketPlatFormServer.DTO.Payment;

/// <summary>
/// 토스페이먼츠 Webhook 이벤트 DTO
/// 공식 문서: https://docs.tosspayments.com/guides/webhook
/// </summary>
public class TossWebhookDto
{
    /// <summary>
    /// 이벤트 타입
    /// - PAYMENT_STATUS_CHANGED: 결제 상태 변경
    /// </summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 이벤트 생성 시각 (ISO 8601)
    /// </summary>
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = null!;

    /// <summary>
    /// 결제 정보
    /// </summary>
    [JsonPropertyName("data")]
    public TossPaymentResponseDto Data { get; set; } = null!;
}

/// <summary>
/// Webhook 검증을 위한 응답
/// </summary>
public class TossWebhookResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
