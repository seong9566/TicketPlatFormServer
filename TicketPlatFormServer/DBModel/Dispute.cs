using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Dispute
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public long ClaimantId { get; set; }

    public string Type { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User Claimant { get; set; } = null!;

    public virtual ICollection<DisputeEvidence> DisputeEvidences { get; set; } = new List<DisputeEvidence>();

    public virtual Transaction Transaction { get; set; } = null!;
}
