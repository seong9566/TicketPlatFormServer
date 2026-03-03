using TicketPlatFormServer.DTO.Payment;

namespace TicketPlatFormServer.Services.Payment;

/// <summary>
/// 토스페이먼츠 API 서비스 인터페이스
/// </summary>
public interface ITossPaymentsService
{
    /// <summary>
    /// 결제 승인 (토스페이먼츠 API 호출)
    /// </summary>
    Task<TossPaymentResponseDto> ConfirmPaymentAsync(string paymentKey, string orderId, int amount);

    /// <summary>
    /// 결제 조회
    /// </summary>
    Task<TossPaymentResponseDto> GetPaymentAsync(string paymentKey);

    /// <summary>
    /// OrderId로 결제 조회
    /// </summary>
    Task<TossPaymentResponseDto> GetPaymentByOrderIdAsync(string orderId);

    /// <summary>
    /// 결제 취소 (환불)
    /// </summary>
    Task<TossPaymentResponseDto> CancelPaymentAsync(string paymentKey, string cancelReason, int? cancelAmount = null);

    Task<TransferResponseDto> RequestTransferAsync(TransferRequestDto request);

    /// <summary>
    /// 지급대행 잔액 조회 (GET /v2/balances)
    /// </summary>
    Task<PayoutBalanceDto> GetPayoutBalanceAsync();

    Task<TransferStatusDto> GetTransferStatusAsync(string transferId);

    Task<bool> ValidateBankAccountAsync(string bankCode, string accountNumber);

    Task<bool> VerifyBankAccountHolderNameAsync(string bankCode, string accountNumber, string holderName);
}
