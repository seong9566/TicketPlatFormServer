namespace TicketPlatFormServer.DTO.Search;

public class SearchResDto
{
    public List<SearchEventItemDto> Events { get; set; } = new();
    public List<SearchTicketItemDto> Tickets { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class SearchEventItemDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public int? MinPrice { get; set; }
}

public class SearchTicketItemDto
{
    public int TicketId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int Price { get; set; }
    public string? SeatInfo { get; set; }
    public string Status { get; set; } = string.Empty;
}
