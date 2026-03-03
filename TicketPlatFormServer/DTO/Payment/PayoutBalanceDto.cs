namespace TicketPlatFormServer.DTO.Payment;

/// <summary>
/// 토스페이먼츠 지급대행 잔액 조회 응답
/// GET /v2/balances
/// </summary>
public class PayoutBalanceDto
{
    public PayoutBalanceAmountDto? AvailableAmount { get; set; }
    public PayoutBalanceAmountDto? PendingAmount { get; set; }
}

public class PayoutBalanceAmountDto
{
    public string Currency { get; set; } = "KRW";
    public long Value { get; set; }
}
