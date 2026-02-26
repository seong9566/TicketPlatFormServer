namespace TicketPlatFormServer.Repository.ReadModels;

public class SearchTicketReadModel
{
    public int TicketId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int Price { get; set; }
    public string? SeatInfo { get; set; }
    public string Status { get; set; } = string.Empty;
}
