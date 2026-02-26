using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 은행 계좌 정보 테이블
/// </summary>
public partial class BankAccount
{
    public long Id { get; set; }

    public int UserId { get; set; }

    /// <summary>
    /// 은행명
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// 은행 코드
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// 예금주
    /// </summary>
    public string? AccountHolder { get; set; }

    /// <summary>
    /// 계좌 인증 여부
    /// </summary>
    public bool? Verified { get; set; }

    /// <summary>
    /// 1원 인증 코드
    /// </summary>
    public string? VerificationCode { get; set; }

    /// <summary>
    /// 인증 코드 만료 시각
    /// </summary>
    public DateTime? VerificationExpiresAt { get; set; }

    /// <summary>
    /// 인증 완료 시각
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>계좌 인증 Provider (CUSTOM|TOSS|HYBRID)</summary>
    public string? VerificationProvider { get; set; }

    /// <summary>계좌 인증 Tier (TIER_0_NONE|TIER_1_CONTROL_PROOF|TIER_2_ACCOUNT_VALID|TIER_3_REAL_NAME_MATCH)</summary>
    public string? VerificationTier { get; set; }

    /// <summary>계좌 인증 상태 (UNVERIFIED|PENDING|VERIFIED|FAILED|EXPIRED)</summary>
    public string? VerificationStatus { get; set; }

    /// <summary>최근 검증 실패 코드</summary>
    public string? LastVerificationFailureCode { get; set; }

    /// <summary>최근 검증 시각</summary>
    public DateTime? LastVerificationAt { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
}
