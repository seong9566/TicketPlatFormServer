using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO.Dispute;

public class AdminResolveDisputeReqDto
{
    [Required]
    public string ResolutionCode { get; set; } = string.Empty;

    [Required]
    public string Reason { get; set; } = string.Empty;
}
