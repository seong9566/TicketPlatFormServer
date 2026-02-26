namespace TicketPlatFormServer.DTO.Withdrawal;

public class WithdrawalResponseDto
{
    public long Id { get; set; }

    public long Amount { get; set; }

    public long Fee { get; set; }

    public long NetAmount { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public string? BankName { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? FailureReason { get; set; }
}
