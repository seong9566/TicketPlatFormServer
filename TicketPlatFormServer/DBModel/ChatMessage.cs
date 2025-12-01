using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class ChatMessage
{
    public long Id { get; set; }

    public long RoomId { get; set; }

    public long SenderId { get; set; }

    public string? Message { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ChatRoom Room { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
