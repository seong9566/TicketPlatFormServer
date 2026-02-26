using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 본인 인증 정보 테이블
/// </summary>
public partial class UserVerification
{
    public int UserId { get; set; }

    /// <summary>
    /// 실명
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 생년월일
    /// </summary>
    public DateOnly? Birth { get; set; }

    /// <summary>
    /// 본인 인증 완료
    /// </summary>
    public bool? IdentityVerified { get; set; }

    /// <summary>
    /// 휴대폰 인증 완료
    /// </summary>
    public bool? PhoneVerified { get; set; }

    /// <summary>
    /// 계좌 인증 완료
    /// </summary>
    public bool? AccountVerified { get; set; }

    /// <summary>
    /// 인증 완료 시각
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
}
