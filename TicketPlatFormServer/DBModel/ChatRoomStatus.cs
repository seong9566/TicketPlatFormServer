using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 채팅방 상태 코드 테이블
/// </summary>
public partial class ChatRoomStatus
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
}
