namespace TicketPlatFormServer.DTO.BankAccount;

public class VerifyAccountResponseDto
{
    public bool Verified { get; set; }

    public string Message { get; set; } = null!;

    public string? Provider { get; set; }

    public string? VerificationStatus { get; set; }

    public string? VerificationTier { get; set; }

    public string? ReasonCode { get; set; }
}
