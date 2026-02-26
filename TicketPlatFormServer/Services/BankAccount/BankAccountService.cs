using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.BankAccount;
using TicketPlatFormServer.Repository.BankAccounts;
using TicketPlatFormServer.Services.Payment;

namespace TicketPlatFormServer.Services.BankAccount;

public class BankAccountService(
    IBankAccountRepository bankAccountRepository,
    ITossPaymentsService tossPaymentsService) : IBankAccountService
{
    public async Task<BankAccountResponseDto> RegisterBankAccountAsync(RegisterBankAccountRequestDto request, long userId)
    {
        if (string.IsNullOrWhiteSpace(request.BankName) ||
            string.IsNullOrWhiteSpace(request.BankCode) ||
            string.IsNullOrWhiteSpace(request.AccountNumber) ||
            string.IsNullOrWhiteSpace(request.AccountHolder))
        {
            throw new AppException("계좌 정보가 올바르지 않습니다.", HttpStatusCode.BadRequest);
        }

        var existing = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        if (existing != null)
        {
            throw new AppException("이미 등록된 계좌가 있습니다.", HttpStatusCode.Conflict);
        }

        var created = await bankAccountRepository.CreateBankAccountAsync(new DBModel.BankAccount
        {
            UserId = (int)userId,
            BankName = request.BankName.Trim(),
            BankCode = request.BankCode.Trim(),
            AccountNumber = request.AccountNumber.Trim(),
            AccountHolder = request.AccountHolder.Trim(),
            Verified = false,
            VerificationCode = null,
            VerificationExpiresAt = null,
            VerifiedAt = null,
            VerificationStatus = "UNVERIFIED",
            VerificationProvider = null,
            VerificationTier = "TIER_0_NONE",
            CreatedAt = DateTime.UtcNow
        });

        var bankCode = created.BankCode ?? string.Empty;
        var accountNumber = created.AccountNumber ?? string.Empty;
        var accountHolder = created.AccountHolder ?? string.Empty;

        try
        {
            var isValidBankAccount = await tossPaymentsService.ValidateBankAccountAsync(bankCode, accountNumber);
            if (!isValidBankAccount)
            {
                await bankAccountRepository.DeleteBankAccountAsync(created.Id);
                throw new AppException("계좌 유효성 검증에 실패했습니다.", HttpStatusCode.BadRequest);
            }

            var isValidAccountHolder = await tossPaymentsService.VerifyBankAccountHolderNameAsync(bankCode, accountNumber, accountHolder);
            if (!isValidAccountHolder)
            {
                await bankAccountRepository.DeleteBankAccountAsync(created.Id);
                throw new AppException("예금주명 검증에 실패했습니다.", HttpStatusCode.BadRequest);
            }
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await bankAccountRepository.DeleteBankAccountAsync(created.Id);
            throw new AppException("계좌 검증 중 오류가 발생했습니다.", HttpStatusCode.BadGateway, ex);
        }

        created.Verified = true;
        created.VerifiedAt = DateTime.UtcNow;
        created.VerificationStatus = "VERIFIED";
        created.VerificationProvider = "TOSS";
        created.VerificationTier = "TIER_2_ACCOUNT_VALID";
        await bankAccountRepository.UpdateBankAccountAsync(created);

        return ToResponse(created);
    }

    public async Task<BankAccountResponseDto?> GetMyBankAccountAsync(long userId)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        return bankAccount == null ? null : ToResponse(bankAccount);
    }
    public async Task DeleteBankAccountAsync(long userId)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        if (bankAccount == null)
        {
            throw new AppException("등록된 계좌가 없습니다.", HttpStatusCode.NotFound);
        }

        var hasBlockingSettlement = await bankAccountRepository.HasPendingOrProcessingSettlementsAsync(bankAccount.Id);
        if (hasBlockingSettlement)
        {
            throw new AppException("진행 중인 정산이 있어 계좌를 삭제할 수 없습니다.", HttpStatusCode.Conflict);
        }

        await bankAccountRepository.DeleteBankAccountAsync(bankAccount.Id);
    }

    private static BankAccountResponseDto ToResponse(DBModel.BankAccount bankAccount)
    {
        return new BankAccountResponseDto
        {
            Id = bankAccount.Id,
            BankName = bankAccount.BankName ?? string.Empty,
            BankCode = bankAccount.BankCode ?? string.Empty,
            AccountNumber = bankAccount.AccountNumber ?? string.Empty,
            AccountHolder = bankAccount.AccountHolder ?? string.Empty,
            Verified = bankAccount.Verified == true,
            VerifiedAt = bankAccount.VerifiedAt
        };
    }

}
