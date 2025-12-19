using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Services.Event;

namespace TicketPlatFormServer.Controllers;

/// <summary>
/// 이벤트 관련 컨트롤러
/// </summary>
[ApiController]
[Route("api/events")]
public class EventController : Controller
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    { 
        _eventService = eventService;
    } 
 
    /// <summary>
    /// 카테고리별 공연 목록 조회 (공연 기준)
    /// </summary>
    /// <param name="categoryId">카테고리 ID</param>
    /// <returns>공연 목록</returns>
    [HttpGet]
    public async Task<IActionResult> GetEventsByCategory([FromQuery] int categoryId)
    {
        var result = await _eventService.GetEventsByCategoryId(categoryId);
        
        var resp = new ApiResponse<List<EventListRespDto>>(
            message: "공연 목록 조회 성공",
            data: result,
            statusCode: 200
        );

        return Ok(resp);
    }
}

