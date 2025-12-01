using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class TransactionHistory
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public long? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
