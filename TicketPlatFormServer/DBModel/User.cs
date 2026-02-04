using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 기본 정보 테이블
/// </summary>
public partial class User
{
    public int Id { get; set; }

    /// <summary>
    /// 이메일 (로그인 ID)
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 비밀번호 해시 (소셜 로그인 시 NULL)
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// 연락처
    /// </summary>
    public string? Phone { get; set; }

    public int ProviderId { get; set; }

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 마지막 로그인 시각
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 탈퇴 여부 (Soft Delete)
    /// </summary>
    public bool? IsDeleted { get; set; }

    public virtual AuthProvider Provider { get; set; } = null!;

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual AuthRole Role { get; set; } = null!;
}
