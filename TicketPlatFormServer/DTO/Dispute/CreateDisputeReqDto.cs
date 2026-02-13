using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.Dispute;

public class CreateDisputeReqDto
{
    [Required]
    public long TransactionId { get; set; }

    [Required]
    public string TypeCode { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string Description { get; set; } = string.Empty;
}
