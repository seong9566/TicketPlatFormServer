namespace TicketPlatFormServer.DTO.Payment;

/// <summary>
/// 결제 요청 DTO (클라이언트 → 백엔드)
/// </summary>
public class PaymentRequestDto
{
    /// <summary>
    /// 거래 ID (티켓 구매)
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 결제 금액
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 주문명
    /// </summary>
    public string OrderName { get; set; } = null!;

    /// <summary>
    /// 구매자 이름
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 구매자 이메일
    /// </summary>
    public string? CustomerEmail { get; set; }
}

/// <summary>
/// 결제 요청 응답 DTO (백엔드 → 클라이언트)
/// </summary>
public class PaymentRequestResponseDto
{
    /// <summary>
    /// 주문 ID (토스페이먼츠에서 사용)
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 결제 금액
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 주문명
    /// </summary>
    public string OrderName { get; set; } = null!;

    /// <summary>
    /// 구매자 이름
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 구매자 이메일
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// 성공 리다이렉트 URL
    /// </summary>
    public string SuccessUrl { get; set; } = null!;

    /// <summary>
    /// 실패 리다이렉트 URL
    /// </summary>
    public string FailUrl { get; set; } = null!;
}
