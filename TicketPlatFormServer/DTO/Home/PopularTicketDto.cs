namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 인기 티켓 DTO
/// </summary>
public class PopularTicketDto
{
    public int TicketId { get; set; }
    public string TicketTitle { get; set; } = null!;
    public int Price { get; set; }
    public string? PosterImageUrl { get; set; }
    public string? EventTitle { get; set; }
    public string EventDate { get; set; } = null!;
}

