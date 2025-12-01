using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Notification
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public string? Title { get; set; }

    public string? Body { get; set; }

    public bool? ReadFlag { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? Data { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
