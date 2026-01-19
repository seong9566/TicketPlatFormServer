using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// Refresh Token 저장 테이블
/// </summary>
public partial class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;
}
