using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 기본 정보 테이블
/// </summary>
public partial class User
{
    public long Id { get; set; }

    /// <summary>
    /// 이메일 (로그인 ID)
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 비밀번호 해시 (소셜 로그인 시 NULL)
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// 연락처
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 인증 제공자 FK
    /// </summary>
    public long ProviderId { get; set; }

    /// <summary>
    /// 사용자 역할 FK
    /// </summary>
    public long RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 마지막 로그인 시각
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 탈퇴 여부 (Soft Delete)
    /// </summary>
    public bool? IsDeleted { get; set; }

    public virtual ICollection<AdminAction> AdminActions { get; set; } = new List<AdminAction>();

    public virtual ICollection<ArtistFollower> ArtistFollowers { get; set; } = new List<ArtistFollower>();

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatRoom> ChatRoomBuyers { get; set; } = new List<ChatRoom>();

    public virtual ICollection<ChatRoom> ChatRoomSellers { get; set; } = new List<ChatRoom>();

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<NotificationToken> NotificationTokens { get; set; } = new List<NotificationToken>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual AuthProvider Provider { get; set; } = null!;

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual AuthRole Role { get; set; } = null!;

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
