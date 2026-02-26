namespace TicketPlatFormServer.DTO.BankAccount;

public class UnmaskedAccountResponseDto
{
    public string AccountNumber { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string BankCode { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;
}
