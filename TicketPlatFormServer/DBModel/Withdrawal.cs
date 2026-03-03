using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Withdrawal
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long BankAccountId { get; set; }

    public long Amount { get; set; }

    public long Fee { get; set; }

    public long NetAmount { get; set; }

    public long StatusId { get; set; }

    public string? IdempotencyKey { get; set; }

    public string? PayoutId { get; set; }

    public string? FailureReason { get; set; }

    public int? RetryCount { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual BankAccount BankAccount { get; set; } = null!;

    public virtual WithdrawalStatus Status { get; set; } = null!;
}
