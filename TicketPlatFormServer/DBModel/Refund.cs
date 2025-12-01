using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Refund
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public long PaymentId { get; set; }

    public int Amount { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public long RequestedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual User RequestedByNavigation { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
