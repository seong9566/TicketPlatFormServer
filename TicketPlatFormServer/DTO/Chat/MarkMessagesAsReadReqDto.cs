namespace TicketPlatFormServer.DTO.Chat;

/// <summary>
/// 메시지 읽음 처리 요청 DTO
/// </summary>
public class MarkMessagesAsReadReqDto
{
    /// <summary>
    /// 채팅방 ID
    /// </summary>
    public long RoomId { get; set; }
}
