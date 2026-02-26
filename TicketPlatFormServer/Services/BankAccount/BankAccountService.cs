using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DTO.BankAccount;
using TicketPlatFormServer.Repository.BankAccounts;
using TicketPlatFormServer.Services.Payment;

namespace TicketPlatFormServer.Services.BankAccount;

public class BankAccountService(
    IBankAccountRepository bankAccountRepository,
    IPaymentService paymentService,
    TossPaymentsSettings settings,
    IBankAccountVerificationProviderFactory providerFactory,
    ILogger<BankAccountService> logger) : IBankAccountService
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

        return ToResponse(created);
    }

    public async Task<BankAccountResponseDto?> GetMyBankAccountAsync(long userId)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        return bankAccount == null ? null : ToResponse(bankAccount);
    }

    public async Task<UnmaskedAccountResponseDto> GetUnmaskedAccountNumberAsync(long userId)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        if (bankAccount == null)
        {
            throw new AppException("등록된 계좌가 없습니다.", HttpStatusCode.NotFound);
        }

        if (bankAccount.Verified != true)
        {
            throw new AppException("인증된 계좌만 조회 가능", HttpStatusCode.BadRequest);
        }

        return new UnmaskedAccountResponseDto
        {
            AccountNumber = bankAccount.AccountNumber ?? string.Empty,
            BankName = bankAccount.BankName ?? string.Empty,
            BankCode = bankAccount.BankCode ?? string.Empty,
            AccountHolder = bankAccount.AccountHolder ?? string.Empty,
        };
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

    public async Task<RequestVerificationResponseDto> RequestVerificationAsync(long userId)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        if (bankAccount == null)
        {
            throw new AppException("등록된 계좌가 없습니다.", HttpStatusCode.NotFound);
        }

        var provider = providerFactory.Resolve(settings.BankVerificationProvider);

        var input = new VerificationRequestInput(
            bankAccount.BankCode ?? string.Empty,
            bankAccount.AccountNumber ?? string.Empty,
            bankAccount.AccountHolder ?? string.Empty,
            userId);

        var result = await provider.RequestAsync(input);

        bankAccount.VerificationCode = result.VerificationCode;
        bankAccount.VerificationExpiresAt = result.ExpiresAt;
        bankAccount.Verified = false;
        bankAccount.VerifiedAt = null;
        bankAccount.VerificationProvider = provider.Name;
        bankAccount.VerificationTier = result.VerificationTier;
        bankAccount.VerificationStatus = "PENDING";
        bankAccount.LastVerificationFailureCode = null;

        await bankAccountRepository.UpdateBankAccountAsync(bankAccount);

        return new RequestVerificationResponseDto
        {
            ExpiresAt = result.ExpiresAt,
            Message = "1원 인증 코드가 발급되었습니다.",
            Provider = provider.Name,
            VerificationStatus = "PENDING",
            VerificationTier = result.VerificationTier,
            ReasonCode = result.ReasonCode
        };
    }

    public async Task<VerifyAccountResponseDto> ConfirmVerificationAsync(VerifyAccountRequestDto request, long userId)
    {
        var bankAccount = await bankAccountRepository.GetBankAccountByUserIdAsync(userId);
        if (bankAccount == null)
        {
            throw new AppException("등록된 계좌가 없습니다.", HttpStatusCode.NotFound);
        }

        // PENDING 상태 확인 (신규 필드 또는 기존 VerificationCode 방식 모두 지원)
        var hasPendingRequest = bankAccount.VerificationStatus == "PENDING" ||
                                !string.IsNullOrWhiteSpace(bankAccount.VerificationCode);
        if (!hasPendingRequest)
        {
            throw new AppException("인증 코드 요청 이력이 없습니다.", HttpStatusCode.BadRequest);
        }

        // 만료 시각 검사 (Custom 방식에서만 ExpiresAt이 설정됨)
        if (bankAccount.VerificationExpiresAt.HasValue && bankAccount.VerificationExpiresAt.Value < DateTime.UtcNow)
        {
            throw new AppException("인증 코드가 만료되었습니다.", HttpStatusCode.BadRequest);
        }

        var provider = providerFactory.Resolve(settings.BankVerificationProvider);

        var input = new VerificationConfirmInput(
            request.Code,
            bankAccount.VerificationCode,
            bankAccount.VerificationExpiresAt,
            userId);

        var result = await provider.ConfirmAsync(input);

        if (!result.Verified)
        {
            if (result.ReasonCode == "MAX_ATTEMPTS_EXCEEDED")
            {
                bankAccount.VerificationCode = null;
                bankAccount.VerificationExpiresAt = null;
                bankAccount.VerificationStatus = "FAILED";
                bankAccount.LastVerificationFailureCode = "MAX_ATTEMPTS_EXCEEDED";
                await bankAccountRepository.UpdateBankAccountAsync(bankAccount);

                logger.LogWarning("[BankAccountService.ConfirmVerificationAsync] 최대 시도 횟수 초과. UserId={UserId}", userId);
                throw new AppException("인증 시도 횟수를 초과했습니다. 다시 요청해주세요.", HttpStatusCode.BadRequest);
            }

            throw new AppException("인증 코드가 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        bankAccount.Verified = true;
        bankAccount.VerifiedAt = DateTime.UtcNow;
        bankAccount.VerificationCode = null;
        bankAccount.VerificationExpiresAt = null;
        bankAccount.VerificationStatus = "VERIFIED";
        bankAccount.VerificationTier = result.VerificationTier;
        bankAccount.LastVerificationAt = DateTime.UtcNow;

        await bankAccountRepository.UpdateBankAccountAsync(bankAccount);

        await paymentService.ResumeHeldSettlementsAsync(userId, bankAccount.Id);

        return new VerifyAccountResponseDto
        {
            Verified = true,
            Message = "계좌 인증이 완료되었습니다.",
            Provider = provider.Name,
            VerificationStatus = "VERIFIED",
            VerificationTier = result.VerificationTier,
            ReasonCode = null
        };
    }

    private static BankAccountResponseDto ToResponse(DBModel.BankAccount bankAccount)
    {
        return new BankAccountResponseDto
        {
            Id = bankAccount.Id,
            BankName = bankAccount.BankName ?? string.Empty,
            BankCode = bankAccount.BankCode ?? string.Empty,
            AccountNumber = MaskAccountNumber(bankAccount.AccountNumber ?? string.Empty),
            AccountHolder = bankAccount.AccountHolder ?? string.Empty,
            Verified = bankAccount.Verified == true,
            VerifiedAt = bankAccount.VerifiedAt
        };
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length <= 4)
        {
            return "****";
        }

        var visible = accountNumber[^4..];
        return new string('*', Math.Max(0, accountNumber.Length - 4)) + visible;
    }
}
