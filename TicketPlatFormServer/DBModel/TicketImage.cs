using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class TicketImage
{
    public long Id { get; set; }

    public long TicketId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
