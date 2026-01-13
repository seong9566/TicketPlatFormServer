using Microsoft.AspNetCore.Http;

namespace TicketPlatFormServer.DTO.Chat;

public class SendMessageReqDto
{
    public long RoomId { get; set; }
    public int UserId { get; set; }
    public string? Message { get; set; }
    public IFormFile? Image { get; set; }
}
