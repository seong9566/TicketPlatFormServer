using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DTO.Chat;

/// <summary>
/// 메시지 전송 응답 DTO
/// </summary>
public class SendMessageRespDto
{
    public long MessageId { get; set; }
    public long RoomId { get; set; }
    public int SenderId { get; set; }
    public string? ClientMessageId { get; set; }
    public string SenderNickname { get; set; } = null!;
    public string? SenderProfileImage { get; set; }
    public string? Message { get; set; }
    public string Type { get; set; } = null!;
    
    /// <summary>
    /// 첨부된 이미지들
    /// </summary>
    public List<ImageInfo>? Images { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// 이미지 정보
/// </summary>
public class ImageInfo
{
    /// <summary>
    /// Signed URL
    /// </summary>
    public string Url { get; set; } = null!;
    
    /// <summary>
    /// URL 만료 시간
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
