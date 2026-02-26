namespace TicketPlatFormServer.Repository.ReadModels;

public class SearchEventReadModel
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public int? MinPrice { get; set; }
}
