using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Payment;

/// <summary>
/// 결제(Payment) 및 에스크로(Escrow) 관련 Repository 인터페이스
/// </summary>
public interface IPaymentRepository
{
    // ==================== Payment CRUD ====================

    /// <summary>
    /// 결제 정보 생성
    /// </summary>
    /// <param name="payment">결제 정보</param>
    /// <returns>생성된 결제 정보</returns>
    Task<DBModel.Payment> CreatePaymentAsync(DBModel.Payment payment);

    /// <summary>
    /// OrderId로 결제 조회
    /// </summary>
    /// <param name="orderId">주문 ID</param>
    /// <returns>결제 정보 (없으면 null)</returns>
    Task<DBModel.Payment?> GetPaymentByOrderIdAsync(string orderId);

    /// <summary>
    /// PaymentKey로 결제 조회
    /// </summary>
    /// <param name="paymentKey">PG사 결제 키</param>
    /// <returns>결제 정보 (없으면 null)</returns>
    Task<DBModel.Payment?> GetPaymentByPaymentKeyAsync(string paymentKey);

    /// <summary>
    /// TransactionId로 결제 조회
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <returns>결제 정보 (없으면 null)</returns>
    Task<DBModel.Payment?> GetPaymentByTransactionIdAsync(long transactionId);

    /// <summary>
    /// 결제 상태 업데이트
    /// </summary>
    /// <param name="paymentId">결제 ID</param>
    /// <param name="statusId">상태 ID</param>
    /// <param name="paidAt">결제 완료 시각 (null이면 업데이트 안 함)</param>
    Task UpdatePaymentStatusAsync(long paymentId, long statusId, DateTime? paidAt = null);

    // ==================== Escrow 관리 ====================

    /// <summary>
    /// 에스크로 생성
    /// </summary>
    /// <param name="escrow">에스크로 정보</param>
    /// <returns>생성된 에스크로 정보</returns>
    Task<Escrow> CreateEscrowAsync(Escrow escrow);

    /// <summary>
    /// TransactionId로 에스크로 조회
    /// </summary>
    /// <param name="transactionId">거래 ID</param>
    /// <returns>에스크로 정보 (없으면 null)</returns>
    Task<Escrow?> GetEscrowByTransactionIdAsync(long transactionId);

    /// <summary>
    /// 에스크로 해제 (정산)
    /// </summary>
    /// <param name="escrowId">에스크로 ID</param>
    /// <param name="statusId">상태 ID (released)</param>
    /// <param name="releasedAt">정산 완료 시각</param>
    Task<int> ReleaseEscrowAsync(long escrowId, long statusId, long holdingStatusId, DateTime releasedAt);

    /// <summary>
    /// 에스크로 환불
    /// </summary>
    /// <param name="escrowId">에스크로 ID</param>
    /// <param name="statusId">상태 ID (refunded)</param>
    /// <param name="refundedAt">환불 완료 시각</param>
    Task RefundEscrowAsync(long escrowId, long statusId, DateTime refundedAt);

    // ==================== 상태 코드 매핑 (캐싱 권장) ====================

    /// <summary>
    /// Code로 PaymentMethod 조회
    /// </summary>
    /// <param name="code">결제 수단 코드 (예: card, virtual_account)</param>
    /// <returns>PaymentMethod 정보 (없으면 null)</returns>
    Task<PaymentMethod?> GetPaymentMethodByCodeAsync(string code);

    /// <summary>
    /// Code로 PaymentStatus 조회
    /// </summary>
    /// <param name="code">결제 상태 코드 (예: pending, paid, cancelled)</param>
    /// <returns>PaymentStatus 정보 (없으면 null)</returns>
    Task<PaymentStatus?> GetPaymentStatusByCodeAsync(string code);

    /// <summary>
    /// Code로 TransactionStatus 조회
    /// </summary>
    /// <param name="code">거래 상태 코드 (예: pending, paid, confirmed)</param>
    /// <returns>TransactionStatus 정보 (없으면 null)</returns>
    Task<TransactionStatus?> GetTransactionStatusByCodeAsync(string code);

    /// <summary>
    /// Code로 EscrowStatus 조회
    /// </summary>
    /// <param name="code">에스크로 상태 코드 (예: holding, released, refunded)</param>
    /// <returns>EscrowStatus 정보 (없으면 null)</returns>
    Task<EscrowStatus?> GetEscrowStatusByCodeAsync(string code);

    /// <summary>
    /// Code로 SettlementStatus 조회
    /// </summary>
    /// <param name="code">정산 상태 코드 (예: pending, processing, completed, failed)</param>
    /// <returns>SettlementStatus 정보 (없으면 null)</returns>
    Task<SettlementStatus?> GetSettlementStatusByCodeAsync(string code);

    // ==================== 결제 수단별 상세 정보 ====================

    /// <summary>
    /// 카드 결제 상세 정보 생성
    /// </summary>
    Task<PaymentCardDetail> CreateCardDetailAsync(PaymentCardDetail cardDetail);

    /// <summary>
    /// 가상계좌 결제 상세 정보 생성
    /// </summary>
    Task<PaymentVirtualAccountDetail> CreateVirtualAccountDetailAsync(PaymentVirtualAccountDetail vaDetail);

    /// <summary>
    /// 간편결제 상세 정보 생성
    /// </summary>
    Task<PaymentEasyPayDetail> CreateEasyPayDetailAsync(PaymentEasyPayDetail easyPayDetail);

    /// <summary>
    /// 현금영수증 생성
    /// </summary>
    Task<PaymentCashReceipt> CreateCashReceiptAsync(PaymentCashReceipt cashReceipt);

    /// <summary>
    /// 결제 거래 이벤트 생성
    /// </summary>
    Task<PaymentTransaction> CreateTransactionAsync(PaymentTransaction transaction);

    /// <summary>
    /// PaymentId로 카드 상세 정보 조회
    /// </summary>
    Task<PaymentCardDetail?> GetCardDetailByPaymentIdAsync(long paymentId);

    /// <summary>
    /// PaymentId로 가상계좌 상세 정보 조회
    /// </summary>
    Task<PaymentVirtualAccountDetail?> GetVirtualAccountDetailByPaymentIdAsync(long paymentId);

    /// <summary>
    /// PaymentId로 간편결제 상세 정보 조회
    /// </summary>
    Task<PaymentEasyPayDetail?> GetEasyPayDetailByPaymentIdAsync(long paymentId);

    /// <summary>
    /// PaymentId로 거래 이벤트 목록 조회
    /// </summary>
    Task<List<PaymentTransaction>> GetTransactionsByPaymentIdAsync(long paymentId);
}
