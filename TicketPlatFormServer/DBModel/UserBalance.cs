using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class UserBalance
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long Available { get; set; }

    public long Pending { get; set; }

    public long TotalEarned { get; set; }

    public long TotalWithdrawn { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
