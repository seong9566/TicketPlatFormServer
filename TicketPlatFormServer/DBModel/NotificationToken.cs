using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class NotificationToken
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string DeviceToken { get; set; } = null!;

    public string Platform { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
