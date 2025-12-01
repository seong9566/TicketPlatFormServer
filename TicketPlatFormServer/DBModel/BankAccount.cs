using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class BankAccount
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string? BankName { get; set; }

    public string? AccountNumber { get; set; }

    public string? AccountHolder { get; set; }

    public bool? Verified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual User User { get; set; } = null!;
}
