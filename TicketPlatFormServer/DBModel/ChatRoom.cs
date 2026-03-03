using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 채팅방 테이블
/// </summary>
public partial class ChatRoom
{
    public long Id { get; set; }

    /// <summary>
    /// 티켓 FK
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 거래 FK (거래 성사 시)
    /// </summary>
    public long? TransactionId { get; set; }

    /// <summary>
    /// 구매자 FK
    /// </summary>
    public int BuyerId { get; set; }

    /// <summary>
    /// 판매자 FK
    /// </summary>
    public int SellerId { get; set; }

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// 마지막 메시지 시각
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 구매자 읽지 않은 수
    /// </summary>
    public int? UnreadCountBuyer { get; set; }

    /// <summary>
    /// 판매자 읽지 않은 수
    /// </summary>
    public int? UnreadCountSeller { get; set; }

    /// <summary>
    /// 채팅 잠금 시각
    /// </summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>
    /// 채팅 종료 시각
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ChatRoomStatus Status { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }
}
