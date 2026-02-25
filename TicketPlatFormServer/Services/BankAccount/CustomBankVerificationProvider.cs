using Microsoft.Extensions.Caching.Memory;
using TicketPlatFormServer.Config;

namespace TicketPlatFormServer.Services.BankAccount;

/// <summary>
/// 커스텀 1원 인증 코드 방식 계좌 인증 Provider
/// </summary>
public class CustomBankVerificationProvider(
    TossPaymentsSettings settings,
    IMemoryCache memoryCache,
    ILogger<CustomBankVerificationProvider> logger) : IBankAccountVerificationProvider
{
    /// <inheritdoc />
    public string Name => "CUSTOM";

    /// <inheritdoc />
    public Task<VerificationRequestResult> RequestAsync(VerificationRequestInput input, CancellationToken ct = default)
    {
        var code = Random.Shared.Next(0, 10000).ToString("D4");
        var expiryMinutes = settings.VerificationCodeExpiryMinutes > 0 ? settings.VerificationCodeExpiryMinutes : 5;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // 시도 횟수 초기화
        memoryCache.Set(GetAttemptCacheKey(input.UserId), 0, TimeSpan.FromMinutes(expiryMinutes + 5));

        logger.LogInformation("[CustomBankVerificationProvider] 인증 코드 발급. UserId={UserId}", input.UserId);

        return Task.FromResult(new VerificationRequestResult(
            PrecheckPassed: false,
            VerificationTier: "TIER_1_CONTROL_PROOF",
            ReasonCode: null,
            VerificationCode: code,
            ExpiresAt: expiresAt));
    }

    /// <inheritdoc />
    public Task<VerificationConfirmResult> ConfirmAsync(VerificationConfirmInput input, CancellationToken ct = default)
    {
        var maxAttempts = settings.MaxVerificationAttempts > 0 ? settings.MaxVerificationAttempts : 3;
        var attemptKey = GetAttemptCacheKey(input.UserId);
        var attempts = memoryCache.TryGetValue<int>(attemptKey, out var currentAttempts) ? currentAttempts : 0;

        if (!string.Equals(input.VerificationCode, input.Code?.Trim(), StringComparison.Ordinal))
        {
            attempts += 1;
            memoryCache.Set(attemptKey, attempts, TimeSpan.FromMinutes(30));

            if (attempts >= maxAttempts)
            {
                memoryCache.Remove(attemptKey);
                logger.LogWarning("[CustomBankVerificationProvider] 최대 시도 횟수 초과. UserId={UserId}", input.UserId);
                return Task.FromResult(new VerificationConfirmResult(false, "TIER_0_NONE", "MAX_ATTEMPTS_EXCEEDED"));
            }

            return Task.FromResult(new VerificationConfirmResult(false, "TIER_0_NONE", "CODE_MISMATCH"));
        }

        memoryCache.Remove(attemptKey);
        logger.LogInformation("[CustomBankVerificationProvider] 인증 성공. UserId={UserId}", input.UserId);

        return Task.FromResult(new VerificationConfirmResult(true, "TIER_1_CONTROL_PROOF", null));
    }

    private static string GetAttemptCacheKey(long userId) =>
        $"BankAccountVerificationAttempt:{userId}";
}
