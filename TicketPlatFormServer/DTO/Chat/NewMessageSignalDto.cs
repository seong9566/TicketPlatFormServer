using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DTO.Chat;

/// <summary>
/// SignalR 실시간 메시지 전송용 DTO
/// </summary>
public class NewMessageSignalDto
{
    public long MessageId { get; set; }
    public long RoomId { get; set; }
    public int SenderId { get; set; }
    public string SenderNickname { get; set; } = null!;
    public string? SenderProfileImage { get; set; }
    public string? Message { get; set; }
    public string Type { get; set; } = null!;
    
    /// <summary>
    /// 첨부된 이미지들
    /// </summary>
    public List<ImageInfo>? Images { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
