namespace TicketPlatFormServer.Config;

/// <summary>
/// 토스페이먼츠 API 설정
/// </summary>
public class TossPaymentsSettings
{
    /// <summary>
    /// API Secret Key (테스트/운영)
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// Client Key (클라이언트용, 결제창에서 사용)
    /// </summary>
    public string ClientKey { get; set; } = null!;

    /// <summary>
    /// API Base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.tosspayments.com";

    /// <summary>
    /// 테스트 모드 여부
    /// </summary>
    public bool IsTestMode { get; set; } = true;

    /// <summary>
    /// 결제 성공 리다이렉트 URL
    /// </summary>
    public string SuccessUrl { get; set; } = null!;

    /// <summary>
    /// 결제 실패 리다이렉트 URL
    /// </summary>
    public string FailUrl { get; set; } = null!;

    /// <summary>
    /// API 타임아웃 (초)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 에스크로 수수료율 (%)
    /// </summary>
    public decimal EscrowFeePercentage { get; set; } = 3.5m;

    public string SettlementApiBaseUrl { get; set; } = "https://api.tosspayments.com";

    public string SettlementCallbackUrl { get; set; } = string.Empty;

    public int MaxSettlementRetryCount { get; set; } = 3;

    public int SettlementProcessingIntervalMinutes { get; set; } = 5;

    public int VerificationCodeExpiryMinutes { get; set; } = 5;

    public int MaxVerificationAttempts { get; set; } = 3;

    public string BankVerificationProvider { get; set; } = "Custom";

    public bool BankVerificationFallbackEnabled { get; set; } = true;

    public int BankVerificationTimeoutSeconds { get; set; } = 10;
}
