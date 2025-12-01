using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class TicketPriceHistory
{
    public long Id { get; set; }

    public long TicketId { get; set; }

    public int OldPrice { get; set; }

    public int NewPrice { get; set; }

    public string? Reason { get; set; }

    public long? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
