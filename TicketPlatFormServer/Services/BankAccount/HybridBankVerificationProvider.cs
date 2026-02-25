using TicketPlatFormServer.Config;

namespace TicketPlatFormServer.Services.BankAccount;

/// <summary>
/// 토스페이먼츠 우선, 실패 시 커스텀 방식으로 fallback하는 Hybrid Provider
/// </summary>
public class HybridBankVerificationProvider(
    TossBankVerificationProvider tossProvider,
    CustomBankVerificationProvider customProvider,
    TossPaymentsSettings settings,
    ILogger<HybridBankVerificationProvider> logger) : IBankAccountVerificationProvider
{
    /// <inheritdoc />
    public string Name => "HYBRID";

    /// <inheritdoc />
    public async Task<VerificationRequestResult> RequestAsync(VerificationRequestInput input, CancellationToken ct = default)
    {
        if (settings.BankVerificationFallbackEnabled)
        {
            try
            {
                return await tossProvider.RequestAsync(input, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "[HybridBankVerificationProvider] Toss 사전 검증 실패, Custom으로 fallback. UserId={UserId}",
                    input.UserId);
                return await customProvider.RequestAsync(input, ct);
            }
        }

        return await tossProvider.RequestAsync(input, ct);
    }

    /// <inheritdoc />
    public Task<VerificationConfirmResult> ConfirmAsync(VerificationConfirmInput input, CancellationToken ct = default)
    {
        // VerificationCode가 없으면 Toss 경로 (사전 검증 완료), 있으면 Custom 경로 (코드 인증 필요)
        if (string.IsNullOrEmpty(input.VerificationCode))
        {
            logger.LogInformation("[HybridBankVerificationProvider] Toss 경로 인증 확인. UserId={UserId}", input.UserId);
            return tossProvider.ConfirmAsync(input, ct);
        }

        logger.LogInformation("[HybridBankVerificationProvider] Custom 경로 인증 확인. UserId={UserId}", input.UserId);
        return customProvider.ConfirmAsync(input, ct);
    }
}
