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

    public int WithdrawalFee { get; set; } = 0;

    public int MaxDailyWithdrawals { get; set; } = 3;

    public long MaxDailyWithdrawalAmount { get; set; } = 5_000_000;

    public long MinWithdrawalAmount { get; set; } = 1_000;

    public int WithdrawalProcessingIntervalMinutes { get; set; } = 5;

    public int MaxWithdrawalRetryCount { get; set; } = 3;

    /// <summary>
    /// 지급대행 보안 키 (64자 Hex 문자열, 개발자센터 > API 개별 키 > 보안 키)
    /// </summary>
    public string? PayoutEncryptionKey { get; set; }
}
