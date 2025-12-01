using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Payment
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public string? PgProvider { get; set; }

    public string? PaymentKey { get; set; }

    public string? OrderId { get; set; }

    public int Amount { get; set; }

    public string Method { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual Transaction Transaction { get; set; } = null!;
}
