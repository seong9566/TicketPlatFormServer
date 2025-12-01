using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class AdminAction
{
    public long Id { get; set; }

    public long AdminId { get; set; }

    public string ActionType { get; set; } = null!;

    public string TargetType { get; set; } = null!;

    public long TargetId { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Admin { get; set; } = null!;
}
