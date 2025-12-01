using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Transaction
{
    public long Id { get; set; }

    public long TicketId { get; set; }

    public long BuyerId { get; set; }

    public long SellerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ReservedAt { get; set; }

    public DateTime? ReservationExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? AutoConfirmAt { get; set; }

    public string? ConfirmedBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual Escrow? Escrow { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual ICollection<TicketVerification> TicketVerifications { get; set; } = new List<TicketVerification>();

    public virtual ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();

    public virtual ICollection<UserReputation> UserReputations { get; set; } = new List<UserReputation>();
}
