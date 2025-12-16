using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 은행 계좌 정보 테이블
/// </summary>
public partial class BankAccount
{
    public long Id { get; set; }

    public long UserId { get; set; }

    /// <summary>
    /// 은행명
    /// </summary>
    public string? BankName { get; set; }

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

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual User User { get; set; } = null!;
}
