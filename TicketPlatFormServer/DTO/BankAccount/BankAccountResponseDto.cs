namespace TicketPlatFormServer.DTO.BankAccount;

public class BankAccountResponseDto
{
    public long Id { get; set; }

    public string BankName { get; set; } = null!;

    public string BankCode { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public bool Verified { get; set; }

    public DateTime? VerifiedAt { get; set; }
}
