using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 채팅 메시지 이미지 테이블
/// </summary>
public partial class ChatMessageImage
{
    public long Id { get; set; }

    /// <summary>
    /// 메시지 FK
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// Supabase Object Key
    /// </summary>
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// 이미지 순서 (0부터 시작)
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ChatMessage Message { get; set; } = null!;
}
