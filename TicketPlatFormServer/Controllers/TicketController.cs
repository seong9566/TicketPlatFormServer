using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Services.Ticket;

namespace TicketPlatFormServer.Controllers;

/// <summary>
/// 티켓 관련 컨트롤러
/// </summary>
[ApiController]
[Route("api/tickets")]
public class TicketController : Controller
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// 티켓 상세 정보 조회
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>티켓 상세 정보</returns>
    [HttpGet]
    [Route("detail")]
    public async Task<IActionResult> GetTicketDetail([FromQuery] int ticketId)
    {
        var result = await _ticketService.GetTicketDetailById(ticketId);
        
        var resp = new ApiResponse<TicketListRespDto>(
            message: "티켓 상세 정보 조회 성공",
            data: result,
            statusCode: 200
        );

        return Ok(resp);
    }
}
