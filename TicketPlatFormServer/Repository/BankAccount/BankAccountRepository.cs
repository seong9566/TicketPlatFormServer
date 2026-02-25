using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.BankAccounts;

public class BankAccountRepository(TicketContext context) : IBankAccountRepository
{
    public async Task<BankAccount?> GetBankAccountByUserIdAsync(long userId)
    {
        return await context.BankAccounts
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<BankAccount?> GetVerifiedBankAccountByUserIdAsync(long userId)
    {
        return await context.BankAccounts
            .Where(x => x.UserId == userId && x.Verified == true)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccount)
    {
        await context.BankAccounts.AddAsync(bankAccount);
        await context.SaveChangesAsync();
        return bankAccount;
    }

    public async Task UpdateBankAccountAsync(BankAccount bankAccount)
    {
        context.BankAccounts.Update(bankAccount);
        await context.SaveChangesAsync();
    }

    public async Task DeleteBankAccountAsync(long id)
    {
        var bankAccount = await context.BankAccounts.FirstOrDefaultAsync(x => x.Id == id);
        if (bankAccount == null)
        {
            return;
        }

        context.BankAccounts.Remove(bankAccount);
        await context.SaveChangesAsync();
    }

    public async Task<bool> HasPendingOrProcessingSettlementsAsync(long bankAccountId)
    {
        return await context.Settlements
            .Where(x => x.BankAccountId == bankAccountId)
            .AnyAsync(x => x.Status.Code == "pending" || x.Status.Code == "processing");
    }
}
