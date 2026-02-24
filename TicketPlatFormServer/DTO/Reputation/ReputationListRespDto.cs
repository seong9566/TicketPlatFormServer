namespace TicketPlatFormServer.DTO.Reputation;

public class ReputationListRespDto
{
    public IEnumerable<ReputationRespDto> Items { get; set; } = Enumerable.Empty<ReputationRespDto>();
    public int TotalCount { get; set; }
    public float? AverageRating { get; set; }
}
