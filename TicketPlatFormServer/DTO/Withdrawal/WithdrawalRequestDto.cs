namespace TicketPlatFormServer.DTO.Withdrawal;

public class WithdrawalRequestDto
{
    public long Amount { get; set; }

    public long? BankAccountId { get; set; }
}
