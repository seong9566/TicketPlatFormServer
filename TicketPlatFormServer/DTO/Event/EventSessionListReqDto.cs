using System.ComponentModel.DataAnnotations;

namespace TicketPlatFormServer.DTO;

/// <summary>
/// 이벤트 세션 목록 조회 ReqDto
/// </summary>
public class EventSessionListReqDto
{
    [Required]
    public int CategoryId { get; set; }
}

