using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using TicketPlatFormServer.DBModel;
using TicketEntity = TicketPlatFormServer.DBModel.Ticket;

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

    public virtual DbSet<AdminActionType> AdminActionTypes { get; set; }

    public virtual DbSet<AdminTargetType> AdminTargetTypes { get; set; }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<ArtistFollower> ArtistFollowers { get; set; }

    public virtual DbSet<AuthProvider> AuthProviders { get; set; }

    public virtual DbSet<AuthRole> AuthRoles { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<ChatRoomStatus> ChatRoomStatuses { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<DisputeEvidence> DisputeEvidences { get; set; }

    public virtual DbSet<DisputeStatus> DisputeStatuses { get; set; }

    public virtual DbSet<DisputeType> DisputeTypes { get; set; }

    public virtual DbSet<Escrow> Escrows { get; set; }

    public virtual DbSet<EscrowStatus> EscrowStatuses { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventSchedule> EventSchedules { get; set; }

    public virtual DbSet<EventSeatArea> EventSeatAreas { get; set; }

    public virtual DbSet<EventSeatGrade> EventSeatGrades { get; set; }

    public virtual DbSet<EventSeatLocation> EventSeatLocations { get; set; }


    public virtual DbSet<FavoriteType> FavoriteTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationPlatform> NotificationPlatforms { get; set; }

    public virtual DbSet<NotificationToken> NotificationTokens { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<RefundReason> RefundReasons { get; set; }

    public virtual DbSet<RefundStatus> RefundStatuses { get; set; }

    public virtual DbSet<ReputationRatingType> ReputationRatingTypes { get; set; }

    // SeatGrade 및 SeatLocation 마스터 테이블은 통합되어 더 이상 사용되지 않음

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<SettlementStatus> SettlementStatuses { get; set; }

    public virtual DbSet<TicketEntity> Tickets { get; set; }

    public virtual DbSet<TicketCategory> TicketCategories { get; set; }

    public virtual DbSet<TicketFeature> TicketFeatures { get; set; }

    public virtual DbSet<TicketImage> TicketImages { get; set; }

    public virtual DbSet<TicketPriceHistory> TicketPriceHistories { get; set; }

    public virtual DbSet<TicketStatus> TicketStatuses { get; set; }


    public virtual DbSet<TicketVerification> TicketVerifications { get; set; }

    public virtual DbSet<TicketVerificationMethod> TicketVerificationMethods { get; set; }

    public virtual DbSet<TradeMethod> TradeMethods { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionConfirmedBy> TransactionConfirmedBys { get; set; }

    public virtual DbSet<TransactionHistory> TransactionHistories { get; set; }

    public virtual DbSet<TransactionItem> TransactionItems { get; set; }

    public virtual DbSet<TransactionStatus> TransactionStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserReputation> UserReputations { get; set; }

    public virtual DbSet<UserVerification> UserVerifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=TicketPlatFormDB;user=root;password=stecdev1234!;sslmode=None;allowpublickeyretrieval=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdminAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("admin_actions", tb => tb.HasComment("관리자 액션 로그 테이블"));

            entity.HasIndex(e => e.ActionTypeId, "idx_admin_actions_action_type_id");

            entity.HasIndex(e => new { e.AdminId, e.CreatedAt }, "idx_admin_actions_admin");

            entity.HasIndex(e => new { e.TargetTypeId, e.TargetId }, "idx_admin_actions_target");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionTypeId)
                .HasComment("액션 유형 FK")
                .HasColumnName("action_type_id");
            entity.Property(e => e.AdminId)
                .HasComment("관리자 FK")
                .HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Reason)
                .HasComment("사유")
                .HasColumnType("text")
                .HasColumnName("reason");
            entity.Property(e => e.TargetId)
                .HasComment("대상 ID")
                .HasColumnName("target_id");
            entity.Property(e => e.TargetTypeId)
                .HasComment("대상 유형 FK")
                .HasColumnName("target_type_id");

            entity.HasOne(d => d.ActionType).WithMany(p => p.AdminActions)
                .HasForeignKey(d => d.ActionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_admin_actions_action_type");

            entity.HasOne(d => d.TargetType).WithMany(p => p.AdminActions)
                .HasForeignKey(d => d.TargetTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_admin_actions_target_type");
        });

        modelBuilder.Entity<AdminActionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("admin_action_types", tb => tb.HasComment("관리자 액션 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_admin_action_types_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasComment("활성화 여부")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasComment("한글 표시명")
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder)
                .HasComment("정렬 순서")
                .HasColumnName("sort_order");
        });

        modelBuilder.Entity<AdminTargetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("admin_target_types", tb => tb.HasComment("관리자 작업 대상 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_admin_target_types_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasComment("한글 표시명")
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("artists");

            entity.HasIndex(e => e.IsActive, "idx_artists_active");

            entity.HasIndex(e => e.CategoryId, "idx_artists_category");

            entity.HasIndex(e => e.Name, "idx_artists_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(500)
                .HasColumnName("profile_image_url");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Artists)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artists_ibfk_1");
        });

        modelBuilder.Entity<ArtistFollower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("artist_followers");

            entity.HasIndex(e => e.ArtistId, "idx_artist_followers_artist");

            entity.HasIndex(e => e.UserId, "idx_artist_followers_user");

            entity.HasIndex(e => new { e.ArtistId, e.UserId }, "uk_artist_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Artist).WithMany(p => p.ArtistFollowers)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artist_followers_ibfk_1");
        });

        modelBuilder.Entity<AuthProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("auth_providers", tb => tb.HasComment("인증 제공자 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_auth_providers_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<AuthRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("auth_roles", tb => tb.HasComment("사용자 역할 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_auth_roles_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bank_account", tb => tb.HasComment("사용자 은행 계좌 정보 테이블"));

            entity.HasIndex(e => e.UserId, "idx_bank_user");

            entity.HasIndex(e => new { e.UserId, e.Verified }, "idx_bank_verified");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountHolder)
                .HasMaxLength(50)
                .HasComment("예금주")
                .HasColumnName("account_holder");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .HasComment("계좌번호")
                .HasColumnName("account_number");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .HasComment("은행명")
                .HasColumnName("bank_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Verified)
                .HasDefaultValueSql("'0'")
                .HasComment("계좌 인증 여부")
                .HasColumnName("verified");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chat_messages", tb => tb.HasComment("채팅 메시지 테이블"));

            entity.HasIndex(e => e.CreatedAt, "idx_msg_created");

            entity.HasIndex(e => e.RoomId, "idx_msg_room");

            entity.HasIndex(e => new { e.RoomId, e.CreatedAt }, "idx_msg_room_created");

            entity.HasIndex(e => new { e.SenderId, e.CreatedAt }, "idx_msg_sender_created");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasComment("이미지 URL")
                .HasColumnName("image_url");
            entity.Property(e => e.Message)
                .HasComment("메시지 내용")
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.RoomId)
                .HasComment("채팅방 FK")
                .HasColumnName("room_id");
            entity.Property(e => e.SenderId)
                .HasComment("발신자 FK")
                .HasColumnName("sender_id");

            entity.HasOne(d => d.Room).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_chat_messages_room");

            entity.HasOne(d => d.Sender).WithMany()
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chat_rooms", tb => tb.HasComment("채팅방 테이블"));

            entity.HasIndex(e => new { e.BuyerId, e.LastMessageAt }, "idx_chat_buyer_last_msg").IsDescending(false, true);

            entity.HasIndex(e => new { e.BuyerId, e.StatusId }, "idx_chat_buyer_status");

            entity.HasIndex(e => e.DeletedAt, "idx_chat_not_deleted");

            entity.HasIndex(e => e.SellerId, "idx_chat_seller");

            entity.HasIndex(e => new { e.SellerId, e.LastMessageAt }, "idx_chat_seller_last_msg").IsDescending(false, true);

            entity.HasIndex(e => e.StatusId, "idx_chat_status_id");

            entity.HasIndex(e => new { e.TicketId, e.BuyerId }, "idx_chat_ticket_buyer").IsUnique();

            entity.HasIndex(e => e.TransactionId, "idx_chat_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerId)
                .HasComment("구매자 FK")
                .HasColumnName("buyer_id");
            entity.Property(e => e.ClosedAt)
                .HasComment("채팅 종료 시각")
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
                .HasComment("마지막 메시지 시각")
                .HasColumnType("timestamp")
                .HasColumnName("last_message_at");
            entity.Property(e => e.LockedAt)
                .HasComment("채팅 잠금 시각")
                .HasColumnType("datetime")
                .HasColumnName("locked_at");
            entity.Property(e => e.SellerId)
                .HasComment("판매자 FK")
                .HasColumnName("seller_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("상태 FK")
                .HasColumnName("status_id");
            entity.Property(e => e.TicketId)
                .HasComment("티켓 FK")
                .HasColumnName("ticket_id");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK (거래 성사 시)")
                .HasColumnName("transaction_id");
            entity.Property(e => e.UnreadCountBuyer)
                .HasDefaultValueSql("'0'")
                .HasComment("구매자 읽지 않은 수")
                .HasColumnName("unread_count_buyer");
            entity.Property(e => e.UnreadCountSeller)
                .HasDefaultValueSql("'0'")
                .HasComment("판매자 읽지 않은 수")
                .HasColumnName("unread_count_seller");

            entity.HasOne(d => d.Status).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_chat_rooms_status");

            entity.HasOne(d => d.Transaction).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("fk_chat_rooms_trans");

            entity.HasOne(d => d.Buyer).WithMany()
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Seller).WithMany()
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Ticket).WithMany()
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ChatRoomStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("chat_room_statuses", tb => tb.HasComment("채팅방 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_chat_room_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(16)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("disputes", tb => tb.HasComment("분쟁 테이블"));

            entity.HasIndex(e => e.ClaimantId, "idx_dispute_claimant");

            entity.HasIndex(e => e.StatusId, "idx_dispute_status");

            entity.HasIndex(e => e.TransactionId, "idx_dispute_trans");

            entity.HasIndex(e => e.TypeId, "idx_dispute_type_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClaimantId)
                .HasComment("신고자 FK")
                .HasColumnName("claimant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasComment("분쟁 내용")
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("상태 FK")
                .HasColumnName("status_id");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK")
                .HasColumnName("transaction_id");
            entity.Property(e => e.TypeId)
                .HasDefaultValueSql("'4'")
                .HasComment("분쟁 유형 FK")
                .HasColumnName("type_id");

            entity.HasOne(d => d.Status).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disputes_status");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disputes_trans");

            entity.HasOne(d => d.Type).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disputes_type");
        });

        modelBuilder.Entity<DisputeEvidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispute_evidence", tb => tb.HasComment("분쟁 증거 자료 테이블"));

            entity.HasIndex(e => e.DisputeId, "idx_dispute_evidence_dispute");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DisputeId)
                .HasComment("분쟁 FK")
                .HasColumnName("dispute_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasComment("증거 이미지 URL")
                .HasColumnName("image_url");
            entity.Property(e => e.Note)
                .HasComment("설명")
                .HasColumnType("text")
                .HasColumnName("note");

            entity.HasOne(d => d.Dispute).WithMany(p => p.DisputeEvidences)
                .HasForeignKey(d => d.DisputeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dispute_evidence_dispute");
        });

        modelBuilder.Entity<DisputeStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispute_statuses", tb => tb.HasComment("분쟁 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_dispute_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<DisputeType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dispute_types", tb => tb.HasComment("분쟁 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_dispute_types_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<Escrow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("escrow", tb => tb.HasComment("에스크로 (결제 대금 보관) 테이블"));

            entity.HasIndex(e => e.StatusId, "idx_escrow_status_id");

            entity.HasIndex(e => e.TransactionId, "uq_escrow_transaction").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasComment("총 금액")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FeeAmount)
                .HasComment("수수료")
                .HasColumnName("fee_amount");
            entity.Property(e => e.RefundedAt)
                .HasComment("환불 완료 시각")
                .HasColumnType("datetime")
                .HasColumnName("refunded_at");
            entity.Property(e => e.ReleasedAt)
                .HasComment("정산 완료 시각")
                .HasColumnType("datetime")
                .HasColumnName("released_at");
            entity.Property(e => e.SellerAmount)
                .HasComment("판매자 정산 금액")
                .HasColumnName("seller_amount");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("상태 FK")
                .HasColumnName("status_id");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK (1:1)")
                .HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Status).WithMany(p => p.Escrows)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_escrow_status");

            entity.HasOne(d => d.Transaction).WithOne(p => p.Escrow)
                .HasForeignKey<Escrow>(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_escrow_trans");
        });

        modelBuilder.Entity<EscrowStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("escrow_statuses", tb => tb.HasComment("에스크로 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_escrow_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("events", tb => tb.HasComment("이벤트/공연 정보 테이블"));

            entity.HasIndex(e => e.CreatedByAdminId, "fk_events_admin");

            entity.HasIndex(e => e.ArtistId, "idx_events_artist");

            entity.HasIndex(e => new { e.CategoryId, e.IsActive, e.SortOrder }, "idx_events_category_active_sort");

            entity.HasIndex(e => e.Title, "idx_events_title");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByAdminId).HasColumnName("created_by_admin_id");
            entity.Property(e => e.Description)
                .HasComment("설명")
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EndAt)
                .HasComment("공연 종료 시간")
                .HasColumnType("datetime")
                .HasColumnName("end_at");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasComment("활성화 여부")
                .HasColumnName("is_active");
            entity.Property(e => e.PosterImageUrl)
                .HasMaxLength(500)
                .HasComment("포스터 이미지 URL")
                .HasColumnName("poster_image_url");
            entity.Property(e => e.SortOrder)
                .HasComment("정렬 순서")
                .HasColumnName("sort_order");
            entity.Property(e => e.StartAt)
                .HasComment("공연 시작 시간")
                .HasColumnType("datetime")
                .HasColumnName("start_at");
            entity.Property(e => e.Title)
                .HasComment("공연/이벤트 제목")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.VenueAddress)
                .HasMaxLength(500)
                .HasComment("장소 주소")
                .HasColumnName("venue_address");
            entity.Property(e => e.VenueName)
                .HasMaxLength(255)
                .HasComment("장소명")
                .HasColumnName("venue_name");

            entity.HasOne(d => d.Artist).WithMany(p => p.Events)
                .HasForeignKey(d => d.ArtistId)
                .HasConstraintName("fk_events_artist");

            entity.HasOne(d => d.Category).WithMany(p => p.Events)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_events_category");
        });

        modelBuilder.Entity<EventSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("event_schedules", tb => tb.HasComment("공연 일정 테이블"));

            entity.HasIndex(e => e.ScheduleDate, "idx_schedules_date");

            entity.HasIndex(e => e.EventId, "idx_schedules_event");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasComment("일정 ID (예: sch001)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EventId)
                .HasComment("공연 FK")
                .HasColumnName("event_id");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.ScheduleDate)
                .HasComment("공연 날짜")
                .HasColumnName("schedule_date");
            entity.Property(e => e.ScheduleTime)
                .HasComment("공연 시간")
                .HasColumnType("time")
                .HasColumnName("schedule_time");

            entity.HasOne(d => d.Event).WithMany(p => p.EventSchedules)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_schedules_event");
        });

        modelBuilder.Entity<EventSeatArea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("event_seat_areas", tb => tb.HasComment("공연별 좌석 구역"));

            entity.HasIndex(e => e.EventId, "idx_event");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AreaName)
                .HasMaxLength(50)
                .HasComment("구역명 (F1, 1구역 등)")
                .HasColumnName("area_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Event).WithMany(p => p.EventSeatAreas)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("event_seat_areas_ibfk_1");
        });

        modelBuilder.Entity<EventSeatGrade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("event_seat_grades", tb => tb.HasComment("공연별 좌석 등급 매핑 (마스터 명칭 통합)"));

            entity.HasIndex(e => e.SeatGradeId, "seat_grade_id");

            entity.HasIndex(e => new { e.EventId, e.SeatGradeId }, "uk_event_grade").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.SeatGradeId).HasColumnName("seat_grade_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sort_order");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code")
                .IsRequired();
            entity.Property(e => e.NameKo)
                .HasMaxLength(100)
                .HasColumnName("name_ko")
                .IsRequired();
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.OriginalPrice)
                .HasColumnName("original_price")
                .HasComment("공연별 해당 등급 정가");

            entity.HasOne(d => d.Event).WithMany(p => p.EventSeatGrades)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("event_seat_grades_ibfk_1");

        });

        modelBuilder.Entity<EventSeatLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("event_seat_locations", tb => tb.HasComment("공연별 좌석 위치"));

            entity.HasIndex(e => e.EventId, "idx_event");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LocationName)
                .HasMaxLength(50)
                .HasComment("위치명 (플로어석, 1층 등)")
                .HasColumnName("location_name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Event).WithMany(p => p.EventSeatLocations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("event_seat_locations_ibfk_1");
        });

        modelBuilder.Entity<FavoriteType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("favorite_types", tb => tb.HasComment("찜 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_favorite_types_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notifications", tb => tb.HasComment("알림 테이블"));

            entity.HasIndex(e => e.CreatedAt, "idx_noti_created");

            entity.HasIndex(e => e.ReadFlag, "idx_noti_read");

            entity.HasIndex(e => e.TypeId, "idx_noti_type");

            entity.HasIndex(e => e.UserId, "idx_noti_user");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_noti_user_created");

            entity.HasIndex(e => new { e.UserId, e.TypeId, e.CreatedAt }, "idx_noti_user_type_created");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Body)
                .HasMaxLength(500)
                .HasComment("알림 내용")
                .HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Data)
                .HasComment("추가 데이터 (페이로드)")
                .HasColumnType("json")
                .HasColumnName("data");
            entity.Property(e => e.ReadAt)
                .HasComment("읽은 시각")
                .HasColumnType("timestamp")
                .HasColumnName("read_at");
            entity.Property(e => e.ReadFlag)
                .HasDefaultValueSql("'0'")
                .HasComment("읽음 여부")
                .HasColumnName("read_flag");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasComment("알림 제목")
                .HasColumnName("title");
            entity.Property(e => e.TypeId)
                .HasDefaultValueSql("'1'")
                .HasComment("알림 유형 FK")
                .HasColumnName("type_id");
            entity.Property(e => e.UserId)
                .HasComment("수신자 FK")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Type).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notifications_type");
        });

        modelBuilder.Entity<NotificationPlatform>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_platforms", tb => tb.HasComment("알림 플랫폼 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_notification_platforms_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(16)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<NotificationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_token", tb => tb.HasComment("알림 디바이스 토큰 테이블"));

            entity.HasIndex(e => e.PlatformId, "idx_notification_token_platform_id");

            entity.HasIndex(e => e.UserId, "idx_notification_token_user");

            entity.HasIndex(e => e.DeviceToken, "token").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceToken)
                .HasMaxLength(500)
                .HasComment("FCM/APNs 토큰")
                .HasColumnName("device_token");
            entity.Property(e => e.PlatformId)
                .HasComment("플랫폼 FK")
                .HasColumnName("platform_id");
            entity.Property(e => e.UserId)
                .HasComment("사용자 FK")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Platform).WithMany(p => p.NotificationTokens)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notification_token_platform");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification_types", tb => tb.HasComment("알림 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_notification_types_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(64)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payments", tb => tb.HasComment("결제 정보 테이블"));

            entity.HasIndex(e => e.PaymentKey, "idx_payments_key");

            entity.HasIndex(e => e.MethodId, "idx_payments_method_id");

            entity.HasIndex(e => e.OrderId, "idx_payments_order");

            entity.HasIndex(e => e.StatusId, "idx_payments_status_id");

            entity.HasIndex(e => e.TransactionId, "idx_payments_trans");

            entity.HasIndex(e => new { e.TransactionId, e.StatusId }, "idx_payments_trans_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasComment("결제 금액")
                .HasColumnName("amount");
            entity.Property(e => e.MethodId)
                .HasDefaultValueSql("'1'")
                .HasComment("결제 수단 FK")
                .HasColumnName("method_id");
            entity.Property(e => e.OrderId)
                .HasComment("주문 ID")
                .HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasComment("결제 완료 시각")
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentKey)
                .HasComment("PG사 결제 키")
                .HasColumnName("payment_key");
            entity.Property(e => e.PgProvider)
                .HasMaxLength(50)
                .HasComment("PG사 (예: toss, kakao)")
                .HasColumnName("pg_provider");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("결제 상태 FK")
                .HasColumnName("status_id");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK")
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Method).WithMany(p => p.Payments)
                .HasForeignKey(d => d.MethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payments_method");

            entity.HasOne(d => d.Status).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payments_status");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payments_trans");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payment_methods", tb => tb.HasComment("결제 수단 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_payment_methods_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payment_statuses", tb => tb.HasComment("결제 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_payment_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refresh_tokens", tb => tb.HasComment("Refresh Token 저장 테이블"));

            entity.HasIndex(e => e.ExpiryDate, "idx_expiry");

            entity.HasIndex(e => new { e.UserId, e.Token }, "idx_user_token");

            entity.HasIndex(e => e.Token, "token").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.IsRevoked)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_revoked");
            entity.Property(e => e.ReplacedByToken)
                .HasMaxLength(500)
                .HasColumnName("replaced_by_token");
            entity.Property(e => e.RevokedAt)
                .HasColumnType("datetime")
                .HasColumnName("revoked_at");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_ibfk_1");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refunds", tb => tb.HasComment("환불 정보 테이블"));

            entity.HasIndex(e => e.PaymentId, "idx_refunds_payment");

            entity.HasIndex(e => e.ReasonId, "idx_refunds_reason_id");

            entity.HasIndex(e => e.RequestedBy, "idx_refunds_requested_by");

            entity.HasIndex(e => e.StatusId, "idx_refunds_status_id");

            entity.HasIndex(e => e.TransactionId, "idx_refunds_trans");

            entity.HasIndex(e => new { e.TransactionId, e.StatusId }, "idx_refunds_trans_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasComment("환불 금액")
                .HasColumnName("amount");
            entity.Property(e => e.ApprovedBy)
                .HasComment("승인자 FK")
                .HasColumnName("approved_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId)
                .HasComment("결제 FK")
                .HasColumnName("payment_id");
            entity.Property(e => e.ProcessedAt)
                .HasComment("처리 완료 시각")
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.ReasonId)
                .HasDefaultValueSql("'1'")
                .HasComment("환불 사유 FK")
                .HasColumnName("reason_id");
            entity.Property(e => e.RequestedBy)
                .HasComment("요청자 FK")
                .HasColumnName("requested_by");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("상태 FK")
                .HasColumnName("status_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Payment).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_refunds_payment");

            entity.HasOne(d => d.Reason).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.ReasonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_refunds_reason");

            entity.HasOne(d => d.Status).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_refunds_status");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_refunds_trans");
        });

        modelBuilder.Entity<RefundReason>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refund_reasons", tb => tb.HasComment("환불 사유 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_refund_reasons_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(64)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(128)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<RefundStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refund_statuses", tb => tb.HasComment("환불 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_refund_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<ReputationRatingType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reputation_rating_types", tb => tb.HasComment("평판 평가 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_reputation_rating_types_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(16)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });



        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("settlements", tb => tb.HasComment("정산 정보 테이블"));

            entity.HasIndex(e => e.BankAccountId, "idx_settlements_bank");

            entity.HasIndex(e => new { e.StatusId, e.RetryCount, e.ScheduledAt }, "idx_settlements_failed");

            entity.HasIndex(e => e.ScheduledAt, "idx_settlements_scheduled");

            entity.HasIndex(e => e.SellerId, "idx_settlements_seller");

            entity.HasIndex(e => e.StatusId, "idx_settlements_status");

            entity.HasIndex(e => new { e.StatusId, e.ScheduledAt }, "idx_settlements_status_scheduled");

            entity.HasIndex(e => e.TransactionId, "idx_settlements_trans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasComment("총 금액")
                .HasColumnName("amount");
            entity.Property(e => e.BankAccountId)
                .HasComment("정산 계좌 FK")
                .HasColumnName("bank_account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FailureReason)
                .HasComment("실패 사유")
                .HasColumnType("text")
                .HasColumnName("failure_reason");
            entity.Property(e => e.Fee)
                .HasComment("수수료")
                .HasColumnName("fee");
            entity.Property(e => e.NetAmount)
                .HasComment("순 정산 금액")
                .HasColumnName("net_amount");
            entity.Property(e => e.ProcessedAt)
                .HasComment("정산 완료 시각")
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.RetryCount)
                .HasDefaultValueSql("'0'")
                .HasComment("재시도 횟수")
                .HasColumnName("retry_count");
            entity.Property(e => e.ScheduledAt)
                .HasComment("정산 예정 일시")
                .HasColumnType("datetime")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.SellerId)
                .HasComment("판매자 FK")
                .HasColumnName("seller_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("상태 FK")
                .HasColumnName("status_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlements_bank");

            entity.HasOne(d => d.Status).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlements_status");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlements_trans");
        });

        modelBuilder.Entity<SettlementStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("settlement_statuses", tb => tb.HasComment("정산 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_settlement_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<TicketEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tickets", tb => tb.HasComment("티켓 정보 테이블"));

            entity.HasIndex(e => e.EventId, "fk_tickets_event");

            entity.HasIndex(e => e.AreaId, "fk_tickets_event_seat_area");

            entity.HasIndex(e => e.SeatLocationId, "fk_tickets_seat_location");

            entity.HasIndex(e => new { e.CategoryId, e.StatusId }, "idx_tickets_category_status");

            entity.HasIndex(e => e.CreatedAt, "idx_tickets_created");

            entity.HasIndex(e => e.EventDatetime, "idx_tickets_event_date");

            entity.HasIndex(e => e.HasTicket, "idx_tickets_has_ticket");

            entity.HasIndex(e => new { e.StatusId, e.EventDatetime }, "idx_tickets_list");

            entity.HasIndex(e => e.DeletedAt, "idx_tickets_not_deleted");

            entity.HasIndex(e => e.RemainingQuantity, "idx_tickets_remaining_qty");

            entity.HasIndex(e => e.ScheduleId, "idx_tickets_schedule");

            entity.HasIndex(e => new { e.StatusId, e.EventDatetime, e.Price }, "idx_tickets_search");

            entity.HasIndex(e => e.SeatGradeId, "idx_tickets_seat_grade");

            entity.HasIndex(e => e.SellerId, "idx_tickets_seller");

            entity.HasIndex(e => e.StatusId, "idx_tickets_status");

            entity.HasIndex(e => e.TradeMethodId, "idx_tickets_trade_method");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft Delete 시각")
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasComment("상세 설명")
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EventDatetime)
                .HasComment("공연 일시")
                .HasColumnType("datetime")
                .HasColumnName("event_datetime");
            entity.Property(e => e.EventId)
                .HasComment("공연 FK")
                .HasColumnName("event_id");
            entity.Property(e => e.HasTicket)
                .HasComment("티켓 보유 여부 (1: 보유, 0: 미보유)")
                .HasColumnName("has_ticket");
            entity.Property(e => e.IsConsecutive)
                .HasDefaultValueSql("'0'")
                .HasComment("연석 여부")
                .HasColumnName("is_consecutive");
            entity.Property(e => e.Price)
                .HasComment("판매가")
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasComment("총 수량")
                .HasColumnName("quantity");
            entity.Property(e => e.RemainingQuantity)
                .HasComment("남은 수량")
                .HasColumnName("remaining_quantity");
            entity.Property(e => e.Row)
                .HasMaxLength(20)
                .HasComment("열 (예: 5열)")
                .HasColumnName("row");
            entity.Property(e => e.ScheduleId)
                .HasMaxLength(36)
                .HasComment("일정 FK")
                .HasColumnName("schedule_id");
            entity.Property(e => e.SeatGradeId)
                .HasComment("좌석 등급 ID (VIP, 일반, 지정석 등)")
                .HasColumnName("seat_grade_id");
            entity.Property(e => e.SeatLocationId).HasColumnName("seat_location_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status_id");
            entity.Property(e => e.TradeMethodId)
                .HasComment("거래 방식 ID")
                .HasColumnName("trade_method_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.Property(e => e.FeatureIds)
                .HasColumnType("text")
                .HasColumnName("feature_ids")
                .HasComment("티켓 특이사항 ID 목록 (콤마 구분)");

            entity.HasOne(d => d.Area).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("fk_tickets_event_seat_area");

            entity.HasOne(d => d.Category).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticket_category");

            entity.HasOne(d => d.Event).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_tickets_event");

            entity.HasOne(d => d.SeatGrade).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatGradeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tickets_event_seat_grade");


            entity.HasOne(d => d.SeatLocation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatLocationId)
                .HasConstraintName("tickets_ibfk_2");

            entity.HasOne(d => d.Status).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tickets_status");

            entity.HasOne(d => d.TradeMethod).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TradeMethodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tickets_trade_method");
        });

        modelBuilder.Entity<TicketCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_category", tb => tb.HasComment("티켓 카테고리 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_ticket_category_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(32)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<TicketFeature>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("ticket_features", tb => tb.HasComment("티켓 특이사항 마스터"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Code, "code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("특이사항 코드")
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasComment("설명")
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasComment("영문명")
                .HasColumnName("name_en");
            entity.Property(e => e.NameKo)
                .HasMaxLength(100)
                .HasComment("한글명")
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasComment("정렬 순서")
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TicketImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_images", tb => tb.HasComment("티켓 이미지 테이블"));

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
        });

        modelBuilder.Entity<TicketPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_price_history", tb => tb.HasComment("티켓 가격 변경 이력 테이블"));

            entity.HasIndex(e => e.ChangedBy, "idx_ticket_price_changed_by");

            entity.HasIndex(e => e.TicketId, "idx_ticket_price_ticket");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasComment("변경자 FK")
                .HasColumnName("changed_by");
            entity.Property(e => e.NewPrice)
                .HasComment("변경 후 가격")
                .HasColumnName("new_price");
            entity.Property(e => e.OldPrice)
                .HasComment("변경 전 가격")
                .HasColumnName("old_price");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasComment("변경 사유")
                .HasColumnName("reason");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
        });

        modelBuilder.Entity<TicketStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_statuses", tb => tb.HasComment("티켓 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_ticket_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });


        modelBuilder.Entity<TicketVerification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_verification", tb => tb.HasComment("티켓 검증 테이블"));

            entity.HasIndex(e => e.MethodId, "fk_ticket_verification_method");

            entity.HasIndex(e => e.TransactionId, "idx_verify_trans");

            entity.HasIndex(e => e.VerifiedBy, "idx_verify_verified_by");

            entity.HasIndex(e => new { e.TransactionId, e.MethodId }, "uq_ticket_verification_trans_method").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MethodId)
                .HasComment("검증 방법 FK")
                .HasColumnName("method_id");
            entity.Property(e => e.OcrConfidence)
                .HasComment("OCR 신뢰도")
                .HasColumnName("ocr_confidence");
            entity.Property(e => e.QrCodeHash)
                .HasMaxLength(255)
                .HasComment("QR코드 해시")
                .HasColumnName("qr_code_hash");
            entity.Property(e => e.RawData)
                .HasComment("OCR/QR 원본 데이터")
                .HasColumnType("text")
                .HasColumnName("raw_data");
            entity.Property(e => e.TicketNumber)
                .HasMaxLength(100)
                .HasComment("티켓 번호")
                .HasColumnName("ticket_number");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK")
                .HasColumnName("transaction_id");
            entity.Property(e => e.VerificationResult)
                .HasComment("검증 결과")
                .HasColumnName("verification_result");
            entity.Property(e => e.VerifiedAt)
                .HasComment("검증 시각")
                .HasColumnType("timestamp")
                .HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy)
                .HasComment("검증자 FK (수동 검증 시)")
                .HasColumnName("verified_by");

            entity.HasOne(d => d.Method).WithMany(p => p.TicketVerifications)
                .HasForeignKey(d => d.MethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticket_verification_method");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TicketVerifications)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticket_verification_trans");
        });

        modelBuilder.Entity<TicketVerificationMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ticket_verification_methods", tb => tb.HasComment("티켓 검증 방법 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_ticket_verification_methods_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<TradeMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("trade_methods", tb => tb.HasComment("거래 방식 마스터"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Code, "code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("거래 방식 코드")
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasComment("설명")
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.NameEn)
                .HasMaxLength(50)
                .HasComment("영문명")
                .HasColumnName("name_en");
            entity.Property(e => e.NameKo)
                .HasMaxLength(50)
                .HasComment("한글명")
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder)
                .HasDefaultValueSql("'0'")
                .HasComment("정렬 순서")
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transactions", tb => tb.HasComment("거래 정보 테이블 (하나의 거래에 여러 티켓 항목 가능)"));

            entity.HasIndex(e => e.ConfirmedById, "fk_transactions_confirmed_by");

            entity.HasIndex(e => e.BuyerId, "idx_trans_buyer");

            entity.HasIndex(e => new { e.BuyerId, e.StatusId }, "idx_trans_buyer_status");

            entity.HasIndex(e => e.CreatedAt, "idx_trans_created");

            entity.HasIndex(e => e.DeletedAt, "idx_trans_not_deleted");

            entity.HasIndex(e => e.SellerId, "idx_trans_seller");

            entity.HasIndex(e => new { e.SellerId, e.StatusId }, "idx_trans_seller_status");

            entity.HasIndex(e => e.StatusId, "idx_trans_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AutoConfirmAt)
                .HasComment("자동 확정 예정 시각")
                .HasColumnType("datetime")
                .HasColumnName("auto_confirm_at");
            entity.Property(e => e.BuyerId)
                .HasComment("구매자 FK")
                .HasColumnName("buyer_id");
            entity.Property(e => e.CancelledAt)
                .HasComment("취소 시각")
                .HasColumnType("datetime")
                .HasColumnName("cancelled_at");
            entity.Property(e => e.ConfirmedAt)
                .HasComment("구매 확정 시각")
                .HasColumnType("datetime")
                .HasColumnName("confirmed_at");
            entity.Property(e => e.ConfirmedById)
                .HasComment("확정자 유형 FK")
                .HasColumnName("confirmed_by_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasComment("Soft Delete 시각")
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ReservationExpiresAt)
                .HasComment("예약 만료 시각")
                .HasColumnType("datetime")
                .HasColumnName("reservation_expires_at");
            entity.Property(e => e.ReservedAt)
                .HasComment("예약 시각")
                .HasColumnType("datetime")
                .HasColumnName("reserved_at");
            entity.Property(e => e.SellerId)
                .HasComment("판매자 FK")
                .HasColumnName("seller_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasComment("상태 FK")
                .HasColumnName("status_id");

            entity.HasOne(d => d.ConfirmedBy).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ConfirmedById)
                .HasConstraintName("fk_transactions_confirmed_by");

            entity.HasOne(d => d.Status).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transactions_status");
        });

        modelBuilder.Entity<TransactionConfirmedBy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transaction_confirmed_bys", tb => tb.HasComment("거래 확인자 유형 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_transaction_confirmed_bys_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<TransactionHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transaction_history", tb => tb.HasComment("거래 상태 변경 이력 테이블"));

            entity.HasIndex(e => e.TransactionId, "idx_trans_history_trans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasComment("변경자 FK")
                .HasColumnName("changed_by");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .HasComment("새 상태 코드")
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasComment("이전 상태 코드")
                .HasColumnName("old_status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionHistories)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transaction_history_trans");
        });

        modelBuilder.Entity<TransactionItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transaction_items", tb => tb.HasComment("거래 항목 테이블 (티켓별 구매 정보)"));

            entity.HasIndex(e => e.TicketId, "idx_trans_items_ticket");

            entity.HasIndex(e => e.TransactionId, "idx_trans_items_trans");

            entity.HasIndex(e => new { e.TransactionId, e.TicketId }, "uq_trans_items_trans_ticket").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Quantity)
                .HasComment("구매 수량")
                .HasColumnName("quantity");
            entity.Property(e => e.TicketId)
                .HasComment("티켓 FK")
                .HasColumnName("ticket_id");
            entity.Property(e => e.TotalPrice)
                .HasComment("소계 (단가 × 수량)")
                .HasColumnName("total_price");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK")
                .HasColumnName("transaction_id");
            entity.Property(e => e.UnitPrice)
                .HasComment("단가")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionItems)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_trans_items_trans");
        });

        modelBuilder.Entity<TransactionStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transaction_statuses", tb => tb.HasComment("거래 상태 코드 테이블"));

            entity.HasIndex(e => e.Code, "uq_transaction_statuses_code").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.NameKo)
                .HasMaxLength(64)
                .HasColumnName("name_ko");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users", tb => tb.HasComment("사용자 기본 정보 테이블"));

            entity.HasIndex(e => e.IsDeleted, "idx_users_deleted");

            entity.HasIndex(e => e.Email, "idx_users_email").IsUnique();

            entity.HasIndex(e => e.ProviderId, "idx_users_provider_id");

            entity.HasIndex(e => e.RoleId, "idx_users_role_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasComment("이메일 (로그인 ID)")
                .HasColumnName("email");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("'0'")
                .HasComment("탈퇴 여부 (Soft Delete)")
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastLoginAt)
                .HasComment("마지막 로그인 시각")
                .HasColumnType("timestamp")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasComment("비밀번호 해시 (소셜 로그인 시 NULL)")
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasComment("연락처")
                .HasColumnName("phone");
            entity.Property(e => e.ProviderId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("provider_id");
            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("role_id");

            entity.HasOne(d => d.Provider).WithMany(p => p.Users)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_provider");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_role");

            entity.HasOne(d => d.UserProfile).WithOne()
                .HasForeignKey<UserProfile>(d => d.UserId);
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_favorites", tb => tb.HasComment("사용자 찜 테이블"));

            entity.HasIndex(e => new { e.FavoriteTypeId, e.TargetId }, "idx_user_favorites_type_target");

            entity.HasIndex(e => e.UserId, "idx_user_favorites_user");

            entity.HasIndex(e => new { e.UserId, e.FavoriteTypeId, e.TargetId }, "uk_user_favorite").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FavoriteTypeId).HasColumnName("favorite_type_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.FavoriteType).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.FavoriteTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_favorites_type");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user_profile", tb => tb.HasComment("사용자 프로필 테이블"));

            entity.HasIndex(e => e.Nickname, "idx_user_profile_nickname");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Bio)
                .HasComment("자기소개")
                .HasColumnType("text")
                .HasColumnName("bio");
            entity.Property(e => e.MannerTemperature)
                .HasDefaultValueSql("'36.5'")
                .HasComment("매너 온도 (36.5~99.9)")
                .HasColumnName("manner_temperature");
            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasColumnName("nickname");
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(500)
                .HasComment("프로필 이미지 URL")
                .HasColumnName("profile_image_url");
            entity.Property(e => e.TotalTradeCount)
                .HasDefaultValueSql("'0'")
                .HasComment("총 거래 횟수")
                .HasColumnName("total_trade_count");
        });

        modelBuilder.Entity<UserReputation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_reputation", tb => tb.HasComment("사용자 평판 (리뷰) 테이블"));

            entity.HasIndex(e => e.RatingTypeId, "idx_reputation_rating_type_id");

            entity.HasIndex(e => e.ReviewerId, "idx_reputation_reviewer");

            entity.HasIndex(e => e.TransactionId, "idx_reputation_trans");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_reputation_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasComment("리뷰 내용")
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.RatingTypeId)
                .HasComment("평가 유형 FK")
                .HasColumnName("rating_type_id");
            entity.Property(e => e.ReviewerId)
                .HasComment("평가자 FK")
                .HasColumnName("reviewer_id");
            entity.Property(e => e.Score)
                .HasComment("점수 (1-5)")
                .HasColumnName("score");
            entity.Property(e => e.TransactionId)
                .HasComment("거래 FK")
                .HasColumnName("transaction_id");
            entity.Property(e => e.UserId)
                .HasComment("평가 대상 FK")
                .HasColumnName("user_id");

            entity.HasOne(d => d.RatingType).WithMany(p => p.UserReputations)
                .HasForeignKey(d => d.RatingTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_reputation_rating_type");

            entity.HasOne(d => d.Transaction).WithMany(p => p.UserReputations)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_reputation_trans");
        });

        modelBuilder.Entity<UserVerification>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user_verification", tb => tb.HasComment("사용자 본인 인증 정보 테이블"));

            entity.HasIndex(e => e.AccountVerified, "idx_verif_account");

            entity.HasIndex(e => new { e.IdentityVerified, e.PhoneVerified, e.AccountVerified }, "idx_verif_all_verified");

            entity.HasIndex(e => e.IdentityVerified, "idx_verif_identity");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.AccountVerified)
                .HasDefaultValueSql("'0'")
                .HasComment("계좌 인증 완료")
                .HasColumnName("account_verified");
            entity.Property(e => e.Birth)
                .HasComment("생년월일")
                .HasColumnName("birth");
            entity.Property(e => e.IdentityVerified)
                .HasDefaultValueSql("'0'")
                .HasComment("본인 인증 완료")
                .HasColumnName("identity_verified");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("실명")
                .HasColumnName("name");
            entity.Property(e => e.PhoneVerified)
                .HasDefaultValueSql("'0'")
                .HasComment("휴대폰 인증 완료")
                .HasColumnName("phone_verified");
            entity.Property(e => e.VerifiedAt)
                .HasComment("인증 완료 시각")
                .HasColumnType("timestamp")
                .HasColumnName("verified_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
