using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TicketPlatFormServer.DTO.Dispute;

public class AddDisputeEvidenceReqDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    public string? Note { get; set; }
}
