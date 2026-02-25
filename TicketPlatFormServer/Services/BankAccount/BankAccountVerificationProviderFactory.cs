namespace TicketPlatFormServer.Services.BankAccount;

/// <summary>
/// 계좌 인증 Provider Factory 인터페이스
/// </summary>
public interface IBankAccountVerificationProviderFactory
{
    /// <summary>
    /// 설정값에 따라 적절한 계좌 인증 Provider를 반환합니다.
    /// </summary>
    /// <param name="providerSetting">Provider 설정값 (Custom|Toss|Hybrid)</param>
    IBankAccountVerificationProvider Resolve(string? providerSetting);
}

/// <summary>
/// 계좌 인증 Provider Factory 구현체
/// </summary>
public class BankAccountVerificationProviderFactory(
    CustomBankVerificationProvider customProvider,
    TossBankVerificationProvider tossProvider,
    HybridBankVerificationProvider hybridProvider,
    ILogger<BankAccountVerificationProviderFactory> logger) : IBankAccountVerificationProviderFactory
{
    /// <inheritdoc />
    public IBankAccountVerificationProvider Resolve(string? providerSetting)
    {
        var key = providerSetting?.Trim().ToLowerInvariant();

        return key switch
        {
            "custom" => customProvider,
            "toss" => tossProvider,
            "hybrid" => hybridProvider,
            _ => FallbackToCustom(providerSetting)
        };
    }

    private IBankAccountVerificationProvider FallbackToCustom(string? providerSetting)
    {
        logger.LogWarning(
            "[BankAccountVerificationProviderFactory] 알 수 없는 Provider 설정 '{Setting}', Custom으로 fallback.",
            providerSetting);
        return customProvider;
    }
}
