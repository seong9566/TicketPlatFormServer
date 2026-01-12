namespace TicketPlatFormServer.DTO.Chat;

public class RoomUpdatedSignalDto
{
    public long RoomId { get; set; }
    public string Event { get; set; } = null!;
    public long? TransactionId { get; set; }
    public string? StatusCode { get; set; }
    public string? Message { get; set; }
}
