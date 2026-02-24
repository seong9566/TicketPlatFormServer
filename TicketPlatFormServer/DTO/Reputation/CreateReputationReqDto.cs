using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.Reputation;

public class CreateReputationReqDto
{
    [Required]
    public long TransactionId { get; set; }

    [Range(1, 5)]
    public int Score { get; set; }
}
