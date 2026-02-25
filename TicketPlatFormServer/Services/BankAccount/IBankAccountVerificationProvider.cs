namespace TicketPlatFormServer.Services.BankAccount;

/// <summary>
/// 계좌 인증 요청 입력 모델
/// </summary>
/// <param name="BankCode">은행 코드</param>
/// <param name="AccountNumber">계좌번호</param>
/// <param name="AccountHolder">예금주명</param>
/// <param name="UserId">사용자 ID</param>
public record VerificationRequestInput(
    string BankCode,
    string AccountNumber,
    string AccountHolder,
    long UserId);

/// <summary>
/// 계좌 인증 요청 결과
/// </summary>
/// <param name="PrecheckPassed">사전 검증 통과 여부</param>
/// <param name="VerificationTier">인증 Tier</param>
/// <param name="ReasonCode">결과 코드</param>
/// <param name="VerificationCode">인증 코드 (CUSTOM 방식 전용, TOSS 방식은 null)</param>
/// <param name="ExpiresAt">인증 코드 만료 시각 (CUSTOM 방식 전용, TOSS 방식은 null)</param>
public record VerificationRequestResult(
    bool PrecheckPassed,
    string VerificationTier,
    string? ReasonCode,
    string? VerificationCode,
    DateTime? ExpiresAt);

/// <summary>
/// 계좌 인증 확인 입력 모델
/// </summary>
/// <param name="Code">사용자 입력 코드</param>
/// <param name="VerificationCode">저장된 인증 코드 (DB)</param>
/// <param name="ExpiresAt">인증 코드 만료 시각 (DB)</param>
/// <param name="UserId">사용자 ID</param>
public record VerificationConfirmInput(
    string? Code,
    string? VerificationCode,
    DateTime? ExpiresAt,
    long UserId);

/// <summary>
/// 계좌 인증 확인 결과
/// </summary>
/// <param name="Verified">인증 성공 여부</param>
/// <param name="VerificationTier">인증 Tier</param>
/// <param name="ReasonCode">결과 코드 (CODE_MISMATCH|MAX_ATTEMPTS_EXCEEDED|null)</param>
public record VerificationConfirmResult(
    bool Verified,
    string VerificationTier,
    string? ReasonCode);

/// <summary>
/// 계좌 인증 Provider 인터페이스
/// </summary>
public interface IBankAccountVerificationProvider
{
    /// <summary>Provider 이름 (CUSTOM|TOSS|HYBRID)</summary>
    string Name { get; }

    /// <summary>
    /// 계좌 인증 코드 발급 또는 사전 검증 요청
    /// </summary>
    /// <param name="input">인증 요청 입력</param>
    /// <param name="ct">취소 토큰</param>
    Task<VerificationRequestResult> RequestAsync(VerificationRequestInput input, CancellationToken ct = default);

    /// <summary>
    /// 계좌 인증 확인
    /// </summary>
    /// <param name="input">인증 확인 입력</param>
    /// <param name="ct">취소 토큰</param>
    Task<VerificationConfirmResult> ConfirmAsync(VerificationConfirmInput input, CancellationToken ct = default);
}
