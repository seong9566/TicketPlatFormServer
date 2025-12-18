namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 추천 이벤트 DTO (Just for you)
/// </summary>
public class RecommendedEventDto
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public string? PosterImageUrl { get; set; }
    public string EventDate { get; set; } = null!;
    public int TicketCount { get; set; }
}

