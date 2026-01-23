using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace TicketPlatFormServer.DTO.Chat;

/// <summary>
/// 메시지 전송 요청 DTO
/// </summary>
public class SendMessageReqDto
{
    public long RoomId { get; set; }
    public int UserId { get; set; }
    public string? Message { get; set; }
    
    /// <summary>
    /// 이미지 파일들 (최대 5개)
    /// </summary>
    public List<IFormFile>? Images { get; set; }
}
