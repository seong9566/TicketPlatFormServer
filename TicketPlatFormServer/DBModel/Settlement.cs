using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Settlement
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public long SellerId { get; set; }

    public int Amount { get; set; }

    public int Fee { get; set; }

    public int NetAmount { get; set; }

    public long BankAccountId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? FailureReason { get; set; }

    public int? RetryCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BankAccount BankAccount { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
