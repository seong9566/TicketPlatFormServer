using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 잔고 정보 테이블
/// </summary>
public partial class UserBalance
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long Available { get; set; }

    public long Pending { get; set; }

    public long TotalEarned { get; set; }

    public long TotalWithdrawn { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<BalanceTransaction> BalanceTransactions { get; set; } = new List<BalanceTransaction>();
}
