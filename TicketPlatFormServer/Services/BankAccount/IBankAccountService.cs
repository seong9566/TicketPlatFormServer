using TicketPlatFormServer.DTO.BankAccount;

namespace TicketPlatFormServer.Services.BankAccount;

public interface IBankAccountService
{
    Task<BankAccountResponseDto> RegisterBankAccountAsync(RegisterBankAccountRequestDto request, long userId);

    Task<BankAccountResponseDto?> GetMyBankAccountAsync(long userId);

    Task DeleteBankAccountAsync(long userId);
}
