using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.BankAccounts;


public interface IBankAccountRepository
{
    Task<BankAccount?> GetBankAccountByUserIdAsync(long userId);

    Task<BankAccount?> GetVerifiedBankAccountByUserIdAsync(long userId);

    Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccount);

    Task UpdateBankAccountAsync(BankAccount bankAccount);

    Task DeleteBankAccountAsync(long id);

    Task<bool> HasPendingOrProcessingSettlementsAsync(long bankAccountId);
}
