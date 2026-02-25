namespace TicketPlatFormServer.DTO.BankAccount;

public class RegisterBankAccountRequestDto
{
    public string BankName { get; set; } = null!;

    public string BankCode { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;
}
