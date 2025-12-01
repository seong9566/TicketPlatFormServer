using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Escrow
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public int Amount { get; set; }

    public int FeeAmount { get; set; }

    public int SellerAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
