using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class UserProfile
{
    public long UserId { get; set; }

    public string Nickname { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public string? Bio { get; set; }

    public float? BuyerRating { get; set; }

    public int? BuyerTradeCount { get; set; }

    public virtual User User { get; set; } = null!;
}
