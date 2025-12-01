using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class UserReputation
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long ReviewerId { get; set; }

    public long TransactionId { get; set; }

    public string RatingType { get; set; } = null!;

    public int Score { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Reviewer { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
