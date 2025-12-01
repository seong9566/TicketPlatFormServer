using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class User
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public string Provider { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<AdminAction> AdminActions { get; set; } = new List<AdminAction>();

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatRoom> ChatRoomBuyers { get; set; } = new List<ChatRoom>();

    public virtual ICollection<ChatRoom> ChatRoomSellers { get; set; } = new List<ChatRoom>();

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<NotificationToken> NotificationTokens { get; set; } = new List<NotificationToken>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<TicketPriceHistory> TicketPriceHistories { get; set; } = new List<TicketPriceHistory>();

    public virtual ICollection<TicketVerification> TicketVerifications { get; set; } = new List<TicketVerification>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Transaction> TransactionBuyers { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSellers { get; set; } = new List<Transaction>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual ICollection<UserReputation> UserReputationReviewers { get; set; } = new List<UserReputation>();

    public virtual ICollection<UserReputation> UserReputationUsers { get; set; } = new List<UserReputation>();

    public virtual UserVerification? UserVerification { get; set; }
}
