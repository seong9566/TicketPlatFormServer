using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class DisputeEvidence
{
    public long Id { get; set; }

    public long DisputeId { get; set; }

    public string? ImageUrl { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Dispute Dispute { get; set; } = null!;
}
