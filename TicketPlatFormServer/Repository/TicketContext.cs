using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository;

public partial class TicketContext : DbContext
{
    public TicketContext()
    {
    }

    public TicketContext(DbContextOptions<TicketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminAction> AdminActions { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<DisputeEvidence> DisputeEvidences { get; set; }

    public virtual DbSet<Escrow> Escrows { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationToken> NotificationTokens { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketImage> TicketImages { get; set; }

    public virtual DbSet<TicketPriceHistory> TicketPriceHistories { get; set; }

    public virtual DbSet<TicketVerification> TicketVerifications { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionHistory> TransactionHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserReputation> UserReputations { get; set; }

    public virtual DbSet<UserVerification> UserVerifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=TicketPlatFormDB;user=root;password=stecdev1234!", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdminAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("admin_actions");

            entity.HasIndex(e => new { e.AdminId, e.CreatedAt }, "idx_admin_actions_admin");

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "idx_admin_actions_target");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasColumnType("enum('DISPUTE_RESOLVE','REFUND_APPROVE','USER_SUSPEND','TICKET_DELETE')")
                .HasColumnName("action_type");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Reason)
                .HasColumnType("text")
                .HasColumnName("reason");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasColumnType("enum('USER','TICKET','TRANSACTION','DISPUTE')")
                .HasColumnName("target_type");

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminActions)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admin_actions_ibfk_1");
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bank_account");

            entity.HasIndex(e => e.UserId, "idx_bank_user");

            entity.HasIndex(e => new { e.UserId, e.Verified }, "idx_bank_verified");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountHolder)
                .HasMaxLength(50)
                .HasColumnName("account_holder");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .HasColumnName("account_number");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .HasColumnName("bank_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Verified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("verified");

            entity.HasOne(d => d.User).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bank_account_ibfk_1");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chat_messages");

            entity.HasIndex(e => e.CreatedAt, "idx_msg_created");

            entity.HasIndex(e => e.RoomId, "idx_msg_room");

            entity.HasIndex(e => new { e.RoomId, e.CreatedAt }, "idx_msg_room_created");

            entity.HasIndex(e => e.SenderId, "sender_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Room).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_messages_ibfk_1");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_messages_ibfk_2");
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chat_rooms");

            entity.HasIndex(e => new { e.BuyerId, e.LastMessageAt }, "idx_chat_buyer_last_msg").IsDescending(false, true);

            entity.HasIndex(e => new { e.BuyerId, e.Status }, "idx_chat_buyer_status");

            entity.HasIndex(e => e.DeletedAt, "idx_chat_not_deleted");

            entity.HasIndex(e => e.SellerId, "idx_chat_seller");

            entity.HasIndex(e => new { e.SellerId, e.LastMessageAt }, "idx_chat_seller_last_msg").IsDescending(false, true);

            entity.HasIndex(e => new { e.TicketId, e.BuyerId }, "idx_chat_ticket_buyer").IsUnique();

            entity.HasIndex(e => e.TransactionId, "idx_chat_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.ClosedAt)
                .HasColumnType("datetime")
                .HasColumnName("closed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.LastMessageAt)
                .HasColumnType("timestamp")
                .HasColumnName("last_message_at");
            entity.Property(e => e.LockedAt)
                .HasColumnType("datetime")
                .HasColumnName("locked_at");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'ACTIVE'")
                .HasColumnType("enum('ACTIVE','LOCKED','CLOSED')")
                .HasColumnName("status");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UnreadCountBuyer)
                .HasDefaultValueSql("'0'")
                .HasColumnName("unread_count_buyer");
            entity.Property(e => e.UnreadCountSeller)
                .HasDefaultValueSql("'0'")
                .HasColumnName("unread_count_seller");

            entity.HasOne(d => d.Buyer).WithMany(p => p.ChatRoomBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_rooms_ibfk_2");

            entity.HasOne(d => d.Seller).WithMany(p => p.ChatRoomSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_rooms_ibfk_3");

            entity.HasOne(d => d.Ticket).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_rooms_ibfk_1");

            entity.HasOne(d => d.Transaction).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("chat_rooms_ibfk_4");
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("disputes");

            entity.HasIndex(e => e.ClaimantId, "claimant_id");

            entity.HasIndex(e => e.Status, "idx_dispute_status");

            entity.HasIndex(e => e.TransactionId, "idx_dispute_trans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClaimantId).HasColumnName("claimant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'OPEN'")
                .HasColumnType("enum('OPEN','UNDER_REVIEW','APPROVED','REJECTED')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Type)
                .HasColumnType("enum('FRAUD','WRONG_TICKET','MISLEAING','OTHER')")
                .HasColumnName("type");

            entity.HasOne(d => d.Claimant).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.ClaimantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disputes_ibfk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disputes_ibfk_1");
        });

        modelBuilder.Entity<DisputeEvidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispute_evidence");

            entity.HasIndex(e => e.DisputeId, "idx_dispute_evidence_dispute");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.Note)
                .HasColumnType("text")
                .HasColumnName("note");

            entity.HasOne(d => d.Dispute).WithMany(p => p.DisputeEvidences)
                .HasForeignKey(d => d.DisputeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dispute_evidence_ibfk_1");
        });

        modelBuilder.Entity<Escrow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("escrow");

            entity.HasIndex(e => e.TransactionId, "unique_transaction").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FeeAmount).HasColumnName("fee_amount");
            entity.Property(e => e.RefundedAt)
                .HasColumnType("datetime")
                .HasColumnName("refunded_at");
            entity.Property(e => e.ReleasedAt)
                .HasColumnType("datetime")
                .HasColumnName("released_at");
            entity.Property(e => e.SellerAmount).HasColumnName("seller_amount");
            entity.Property(e => e.Status)
                .HasColumnType("enum('HOLD','RELEASED','FROZEN','REFUND_PENDING','REFUNDED')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Transaction).WithOne(p => p.Escrow)
                .HasForeignKey<Escrow>(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("escrow_ibfk_1");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.CreatedAt, "idx_noti_created");

            entity.HasIndex(e => e.ReadFlag, "idx_noti_read");

            entity.HasIndex(e => e.UserId, "idx_noti_user");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_noti_user_created");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Body)
                .HasMaxLength(500)
                .HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Data)
                .HasColumnType("json")
                .HasColumnName("data");
            entity.Property(e => e.ReadAt)
                .HasColumnType("timestamp")
                .HasColumnName("read_at");
            entity.Property(e => e.ReadFlag)
                .HasDefaultValueSql("'0'")
                .HasColumnName("read_flag");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasColumnType("enum('REQUEST','PAID','VERIFY_REQUEST','CONFIRMED','SETTLED','DISPUTE','REFUND_APPROVED','CHAT_MESSAGE','PRICE_CHANGED','TICKET_EXPIRED')")
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<NotificationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_token");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceToken)
                .HasMaxLength(500)
                .HasColumnName("device_token");
            entity.Property(e => e.Platform)
                .HasColumnType("enum('ios','android','web')")
                .HasColumnName("platform");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_token_ibfk_1");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.HasIndex(e => e.PaymentKey, "idx_payments_key");

            entity.HasIndex(e => e.OrderId, "idx_payments_order");

            entity.HasIndex(e => e.TransactionId, "idx_payments_trans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Method)
                .HasColumnType("enum('card','vbank')")
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentKey).HasColumnName("payment_key");
            entity.Property(e => e.PgProvider)
                .HasMaxLength(50)
                .HasColumnName("pg_provider");
            entity.Property(e => e.Status)
                .HasColumnType("enum('PAID','CANCELLED','REFUNDED')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_ibfk_1");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refunds");

            entity.HasIndex(e => e.Status, "idx_refunds_status");

            entity.HasIndex(e => e.TransactionId, "idx_refunds_trans");

            entity.HasIndex(e => e.PaymentId, "payment_id");

            entity.HasIndex(e => e.RequestedBy, "requested_by");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.Reason)
                .HasColumnType("enum('VERIFICATION_FAILED','WRONG_TICKET','EVENT_CANCELLED','FRAUD_CLAIM','BUYER_CANCEL')")
                .HasColumnName("reason");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'REQUESTED'")
                .HasColumnType("enum('REQUESTED','APPROVED','PROCESSING','COMPLETED','REJECTED')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Payment).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("refunds_ibfk_2");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("refunds_ibfk_3");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("refunds_ibfk_1");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("settlements");

            entity.HasIndex(e => e.BankAccountId, "bank_account_id");

            entity.HasIndex(e => new { e.Status, e.RetryCount, e.ScheduledAt }, "idx_settlements_failed");

            entity.HasIndex(e => e.ScheduledAt, "idx_settlements_scheduled");

            entity.HasIndex(e => e.SellerId, "idx_settlements_seller");

            entity.HasIndex(e => e.Status, "idx_settlements_status");

            entity.HasIndex(e => e.TransactionId, "transaction_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FailureReason)
                .HasColumnType("text")
                .HasColumnName("failure_reason");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.NetAmount).HasColumnName("net_amount");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.RetryCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("retry_count");
            entity.Property(e => e.ScheduledAt)
                .HasColumnType("datetime")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'PENDING'")
                .HasColumnType("enum('PENDING','PROCESSING','COMPLETED','FAILED')")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("settlements_ibfk_3");

            entity.HasOne(d => d.Seller).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("settlements_ibfk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("settlements_ibfk_1");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tickets");

            entity.HasIndex(e => e.Category, "idx_tickets_category");

            entity.HasIndex(e => e.CreatedAt, "idx_tickets_created");

            entity.HasIndex(e => e.EventDatetime, "idx_tickets_event_date");

            entity.HasIndex(e => new { e.Category, e.Status, e.EventDatetime }, "idx_tickets_list");

            entity.HasIndex(e => e.DeletedAt, "idx_tickets_not_deleted");

            entity.HasIndex(e => new { e.Status, e.Category, e.EventDatetime, e.Price }, "idx_tickets_search");

            entity.HasIndex(e => e.SellerId, "idx_tickets_seller");

            entity.HasIndex(e => e.Status, "idx_tickets_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasColumnType("enum('concert','musical','sports','festival','other')")
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EventDatetime)
                .HasColumnType("datetime")
                .HasColumnName("event_datetime");
            entity.Property(e => e.IsContinuous)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_continuous");
            entity.Property(e => e.OriginalPrice).HasColumnName("original_price");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SeatInfo)
                .HasMaxLength(255)
                .HasColumnName("seat_info");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Status)
                .HasColumnType("enum('ON_SALE','RESERVED','SOLD_OUT','DELETED','EXPIRED')")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Seller).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_ibfk_1");
        });

        modelBuilder.Entity<TicketImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_images");

            entity.HasIndex(e => e.TicketId, "idx_ticket_img_ticket");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketImages)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_images_ibfk_1");
        });

        modelBuilder.Entity<TicketPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_price_history");

            entity.HasIndex(e => e.ChangedBy, "changed_by");

            entity.HasIndex(e => e.TicketId, "ticket_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.NewPrice).HasColumnName("new_price");
            entity.Property(e => e.OldPrice).HasColumnName("old_price");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.TicketPriceHistories)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("ticket_price_history_ibfk_2");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketPriceHistories)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_price_history_ibfk_1");
        });

        modelBuilder.Entity<TicketVerification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_verification");

            entity.HasIndex(e => e.TransactionId, "idx_verify_trans");

            entity.HasIndex(e => new { e.TransactionId, e.Method }, "unique_transaction_method").IsUnique();

            entity.HasIndex(e => e.VerifiedBy, "verified_by");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Method)
                .HasColumnType("enum('qr','ocr','number')")
                .HasColumnName("method");
            entity.Property(e => e.OcrConfidence).HasColumnName("ocr_confidence");
            entity.Property(e => e.QrCodeHash)
                .HasMaxLength(255)
                .HasColumnName("qr_code_hash");
            entity.Property(e => e.RawData)
                .HasColumnType("text")
                .HasColumnName("raw_data");
            entity.Property(e => e.TicketNumber)
                .HasMaxLength(100)
                .HasColumnName("ticket_number");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.VerificationResult).HasColumnName("verification_result");
            entity.Property(e => e.VerifiedAt)
                .HasColumnType("timestamp")
                .HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TicketVerifications)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_verification_ibfk_1");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.TicketVerifications)
                .HasForeignKey(d => d.VerifiedBy)
                .HasConstraintName("ticket_verification_ibfk_2");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.BuyerId, "idx_trans_buyer");

            entity.HasIndex(e => new { e.BuyerId, e.Status }, "idx_trans_buyer_status");

            entity.HasIndex(e => e.CreatedAt, "idx_trans_created");

            entity.HasIndex(e => e.DeletedAt, "idx_trans_not_deleted");

            entity.HasIndex(e => e.SellerId, "idx_trans_seller");

            entity.HasIndex(e => new { e.SellerId, e.Status }, "idx_trans_seller_status");

            entity.HasIndex(e => e.Status, "idx_trans_status");

            entity.HasIndex(e => e.TicketId, "idx_trans_ticket");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AutoConfirmAt)
                .HasColumnType("datetime")
                .HasColumnName("auto_confirm_at");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CancelledAt)
                .HasColumnType("datetime")
                .HasColumnName("cancelled_at");
            entity.Property(e => e.ConfirmedAt)
                .HasColumnType("datetime")
                .HasColumnName("confirmed_at");
            entity.Property(e => e.ConfirmedBy)
                .HasColumnType("enum('BUYER','AUTO','ADMIN')")
                .HasColumnName("confirmed_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ReservationExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("reservation_expires_at");
            entity.Property(e => e.ReservedAt)
                .HasColumnType("datetime")
                .HasColumnName("reserved_at");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Status)
                .HasColumnType("enum('REQUESTED','PAID','VERIFIED','COMPLETED','CANCELLED')")
                .HasColumnName("status");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Buyer).WithMany(p => p.TransactionBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_2");

            entity.HasOne(d => d.Seller).WithMany(p => p.TransactionSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_3");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_ibfk_1");
        });

        modelBuilder.Entity<TransactionHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transaction_history");

            entity.HasIndex(e => e.TransactionId, "transaction_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasColumnName("old_status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionHistories)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transaction_history_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.IsDeleted, "idx_users_deleted");

            entity.HasIndex(e => e.Provider, "idx_users_provider");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("timestamp")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Provider)
                .HasDefaultValueSql("'email'")
                .HasColumnType("enum('email','google','kakao','apple')")
                .HasColumnName("provider");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'USER'")
                .HasColumnType("enum('USER','ADMIN','SELLER')")
                .HasColumnName("role");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user_profile");

            entity.HasIndex(e => e.Nickname, "idx_user_profile_nickname");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Bio)
                .HasColumnType("text")
                .HasColumnName("bio");
            entity.Property(e => e.BuyerRating)
                .HasDefaultValueSql("'0'")
                .HasColumnName("buyer_rating");
            entity.Property(e => e.BuyerTradeCount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("buyer_trade_count");
            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasColumnName("nickname");
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(500)
                .HasColumnName("profile_image_url");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_profile_ibfk_1");
        });

        modelBuilder.Entity<UserReputation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_reputation");

            entity.HasIndex(e => e.TransactionId, "idx_reputation_trans");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_reputation_user");

            entity.HasIndex(e => e.ReviewerId, "reviewer_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.RatingType)
                .HasColumnType("enum('SELLER','BUYER')")
                .HasColumnName("rating_type");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.UserReputationReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_reputation_ibfk_2");

            entity.HasOne(d => d.Transaction).WithMany(p => p.UserReputations)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_reputation_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.UserReputationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_reputation_ibfk_1");
        });

        modelBuilder.Entity<UserVerification>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user_verification");

            entity.HasIndex(e => e.AccountVerified, "idx_verif_account");

            entity.HasIndex(e => new { e.IdentityVerified, e.PhoneVerified, e.AccountVerified }, "idx_verif_all_verified");

            entity.HasIndex(e => e.IdentityVerified, "idx_verif_identity");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.AccountVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("account_verified");
            entity.Property(e => e.Birth).HasColumnName("birth");
            entity.Property(e => e.IdentityVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("identity_verified");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.PhoneVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("phone_verified");
            entity.Property(e => e.VerifiedAt)
                .HasColumnType("timestamp")
                .HasColumnName("verified_at");

            entity.HasOne(d => d.User).WithOne(p => p.UserVerification)
                .HasForeignKey<UserVerification>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_verification_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
