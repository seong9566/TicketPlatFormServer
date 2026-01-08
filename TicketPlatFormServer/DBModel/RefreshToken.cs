using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// Refresh Token 저장 테이블
/// </summary>
public partial class RefreshToken
{
    public int Id { get; set; }

    /// <summary>
    /// 사용자 ID (FK → Users.Id)
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Refresh Token (UUID 형식, Unique)
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Token 만료 일시 (UTC)
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Token 생성 일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Token 무효화 여부
    /// </summary>
    public bool? IsRevoked { get; set; }

    /// <summary>
    /// Token 무효화 일시
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// 대체된 Token (Token Rotation 시 이전 Token을 무효화하고 새 Token으로 교체)
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// 사용자 Navigation Property
    /// </summary>
    public virtual User User { get; set; } = null!;
}
