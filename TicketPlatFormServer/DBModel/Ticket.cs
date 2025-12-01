using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Ticket
{
    public long Id { get; set; }

    public long SellerId { get; set; }

    public string Category { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTime EventDatetime { get; set; }

    public string? SeatInfo { get; set; }

    public int Quantity { get; set; }

    public bool? IsContinuous { get; set; }

    public int Price { get; set; }

    public int OriginalPrice { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<TicketImage> TicketImages { get; set; } = new List<TicketImage>();

    public virtual ICollection<TicketPriceHistory> TicketPriceHistories { get; set; } = new List<TicketPriceHistory>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
