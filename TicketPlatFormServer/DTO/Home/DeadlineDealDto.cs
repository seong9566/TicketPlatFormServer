namespace TicketPlatFormServer.DTO.Home;

public class DeadlineDealDto
{
    public int EventId { get; set; }

    public string EventTitle { get; set; } = null!;

    public string EventDate { get; set; } = null!;

    public string Venue { get; set; } = null!;

    public int DaysLeft { get; set; }

    public int MinTicketPrice { get; set; }

    public int OriginalMinTicketPrice { get; set; }

    public int TicketDiscountRate { get; set; }

    public string? PosterImageUrl { get; set; }

    public int AvailableTicketCount { get; set; }

    public int CategoryId { get; set; }
}
