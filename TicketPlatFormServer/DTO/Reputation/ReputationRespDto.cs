namespace TicketPlatFormServer.DTO.Reputation;

public class ReputationRespDto
{
    public long Id { get; set; }
    public string ReviewerNickname { get; set; } = string.Empty;
    public string? ReviewerProfileImageUrl { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}
