using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class WithdrawalStatus
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameKo { get; set; } = null!;

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
