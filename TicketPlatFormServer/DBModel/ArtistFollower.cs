using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class ArtistFollower
{
    public long Id { get; set; }

    public long ArtistId { get; set; }

    public long UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
