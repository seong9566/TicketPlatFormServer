using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class ChatRoom
{
    public long Id { get; set; }

    public long TicketId { get; set; }

    public long? TransactionId { get; set; }

    public long BuyerId { get; set; }

    public long SellerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastMessageAt { get; set; }

    public int? UnreadCountBuyer { get; set; }

    public int? UnreadCountSeller { get; set; }

    public DateTime? LockedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User Seller { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }
}
