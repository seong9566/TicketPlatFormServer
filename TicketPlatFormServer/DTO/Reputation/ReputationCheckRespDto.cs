namespace TicketPlatFormServer.DTO.Reputation;

public class ReputationCheckRespDto
{
    public bool CanReview { get; set; }
    public bool HasReviewed { get; set; }
    public DateTime? ReviewDeadline { get; set; }
}
