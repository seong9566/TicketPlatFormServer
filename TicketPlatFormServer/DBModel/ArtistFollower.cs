using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class ArtistFollower
{
    public int Id { get; set; }

    public int ArtistId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Artist Artist { get; set; } = null!;
}
