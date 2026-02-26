namespace TicketPlatFormServer.Repository.ReadModels;

public class SearchResultReadModel
{
    public List<SearchEventReadModel> Events { get; set; } = new();
    public List<SearchTicketReadModel> Tickets { get; set; } = new();
    public int EventTotalCount { get; set; }
    public int TicketTotalCount { get; set; }
}
