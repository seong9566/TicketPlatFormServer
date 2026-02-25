using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Services.Payment;

namespace TicketPlatFormServer.Services.BankAccount;

/// <summary>
/// 토스페이먼츠 계좌 유효성 및 예금주 검증 방식 Provider
/// </summary>
public class TossBankVerificationProvider(
    ITossPaymentsService tossPaymentsService,
    ILogger<TossBankVerificationProvider> logger) : IBankAccountVerificationProvider
{
    /// <inheritdoc />
    public string Name => "TOSS";

    /// <inheritdoc />
    public async Task<VerificationRequestResult> RequestAsync(VerificationRequestInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.BankCode) ||
            string.IsNullOrWhiteSpace(input.AccountNumber) ||
            string.IsNullOrWhiteSpace(input.AccountHolder))
        {
            throw new AppException("토스 계좌 검증에 필요한 계좌 정보가 부족합니다.", HttpStatusCode.BadRequest);
        }

        var accountValid = await tossPaymentsService.ValidateBankAccountAsync(
            input.BankCode.Trim(),
            input.AccountNumber.Trim());

        if (!accountValid)
        {
            throw new AppException("계좌 유효성 검증에 실패했습니다.", HttpStatusCode.BadRequest);
        }

        var holderValid = await tossPaymentsService.VerifyBankAccountHolderNameAsync(
            input.BankCode.Trim(),
            input.AccountNumber.Trim(),
            input.AccountHolder.Trim());

        if (!holderValid)
        {
            throw new AppException("예금주명과 계좌 정보가 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        logger.LogInformation("[TossBankVerificationProvider] 계좌 사전 검증 완료. UserId={UserId}", input.UserId);

        return new VerificationRequestResult(
            PrecheckPassed: true,
            VerificationTier: "TIER_2_ACCOUNT_VALID",
            ReasonCode: null,
            VerificationCode: null,
            ExpiresAt: null);
    }

    /// <inheritdoc />
    public Task<VerificationConfirmResult> ConfirmAsync(VerificationConfirmInput input, CancellationToken ct = default)
    {
        // Toss Provider는 Request 단계에서 이미 검증 완료 → 항상 성공 반환
        logger.LogInformation("[TossBankVerificationProvider] 인증 확인 (사전 검증 완료). UserId={UserId}", input.UserId);
        return Task.FromResult(new VerificationConfirmResult(true, "TIER_2_ACCOUNT_VALID", null));
    }
}
