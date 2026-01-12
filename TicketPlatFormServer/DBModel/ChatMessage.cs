using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 채팅 메시지 테이블
/// </summary>
public partial class ChatMessage
{
    public long Id { get; set; }

    /// <summary>
    /// 채팅방 FK
    /// </summary>
    public long RoomId { get; set; }

    /// <summary>
    /// 발신자 FK
    /// </summary>
    public long SenderId { get; set; }

    /// <summary>
    /// 메시지 내용
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 이미지 URL
    /// </summary>
    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ChatRoom Room { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
