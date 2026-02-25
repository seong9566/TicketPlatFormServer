using TicketPlatFormServer.DTO.Payment;

namespace TicketPlatFormServer.Services.Payment;

/// <summary>
/// 결제 비즈니스 로직 서비스 인터페이스
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// 결제 요청 준비 (OrderId 생성)
    /// </summary>
    /// <param name="request">결제 요청 정보</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>결제 요청 응답 (OrderId, ClientKey 등)</returns>
    Task<PaymentRequestResponseDto> InitiatePaymentAsync(PaymentRequestDto request, int userId);

    /// <summary>
    /// 결제 승인 처리 (Toss API 호출 + DB 저장)
    /// </summary>
    /// <param name="request">결제 승인 요청 (PaymentKey, OrderId, Amount)</param>
    /// <returns>토스페이먼츠 결제 응답</returns>
    Task<TossPaymentResponseDto> ConfirmPaymentAsync(PaymentConfirmRequestDto request);

    /// <summary>
    /// 에스크로 해제 (구매 확정 시)
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    Task ReleaseEscrowAsync(long transactionId);

    Task ResumeHeldSettlementsAsync(long sellerId, long bankAccountId);

    /// <summary>
    /// 결제 취소 (환불)
    /// </summary>
    /// <param name="request">취소 요청 정보</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>취소 응답</returns>
    Task<PaymentCancelResponseDto> CancelPaymentAsync(PaymentCancelRequestDto request, int userId);

    /// <summary>
    /// Webhook 이벤트 처리
    /// </summary>
    /// <param name="webhook">토스페이먼츠 Webhook 데이터</param>
    Task HandleWebhookAsync(TossWebhookDto webhook);

    /// <summary>
    /// OrderId로 결제 조회
    /// </summary>
    /// <param name="orderId">주문 ID</param>
    /// <returns>결제 정보</returns>
    Task<TossPaymentResponseDto> GetPaymentByOrderIdAsync(string orderId);
}
