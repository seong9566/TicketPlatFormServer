using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketPlatFormServer.DTO.Sell;
using TicketPlatFormServer.Services.Sell;

namespace TicketPlatFormServer.Controllers;

/// <summary>
/// 티켓 판매 API Controller
/// </summary>
[ApiController]
[Route("api/sell")]
[Authorize]
public class SellController(ISellService sellService) : ControllerBase
{
    private readonly ISellService _sellService = sellService;

    /// <summary>
    /// JWT에서 UserId 추출
    /// </summary>
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        }
        return userId;
    }

    /// <summary>
    /// 판매 가능한 카테고리 목록 조회
    /// </summary>
    /// <returns>카테고리 목록</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<CategoryRespDto>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _sellService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// 카테고리별 공연 목록 조회 (페이징)
    /// </summary>
    /// <param name="request">조회 조건</param>
    /// <returns>공연 목록 (페이징)</returns>
    [HttpGet("events")]
    [ProducesResponseType(typeof(SellEventListRespDto), 200)]
    public async Task<IActionResult> GetEvents([FromQuery] SellEventListReqDto request)
    {
        var events = await _sellService.GetEventsAsync(request);
        return Ok(events);
    }

    /// <summary>
    /// 특정 공연의 일정 목록 조회
    /// </summary>
    /// <param name="eventId">공연 ID</param>
    /// <returns>일정 목록</returns>
    [HttpGet("events/{eventId}/schedules")]
    [ProducesResponseType(typeof(EventScheduleRespDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEventSchedules(int eventId)
    {
        var schedules = await _sellService.GetEventSchedulesAsync(eventId);
        return Ok(schedules);
    }

    /// <summary>
    /// 특정 공연의 좌석 옵션 조회
    /// </summary>
    /// <param name="eventId">공연 ID</param>
    /// <returns>좌석 옵션 목록</returns>
    [HttpGet("events/{eventId}/seat-options")]
    [ProducesResponseType(typeof(SeatOptionRespDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSeatOptions(int eventId)
    {
        var options = await _sellService.GetSeatOptionsAsync(eventId);
        return Ok(options);
    }

    /// <summary>
    /// 티켓 판매 등록
    /// </summary>
    /// <param name="request">티켓 판매 등록 정보</param>
    /// <returns>등록 결과</returns>
    [HttpPost("tickets")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateSellTicketRespDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateTicket([FromForm] CreateSellTicketReqDto request)
    {
        var userId = GetUserId();
        var result = await _sellService.CreateTicketAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// 내 판매 티켓 목록 조회
    /// </summary>
    /// <param name="request">조회 조건</param>
    /// <returns>내 티켓 목록 (페이징)</returns>
    [HttpGet("my-tickets")]
    [ProducesResponseType(typeof(MyTicketListRespDto), 200)]
    public async Task<IActionResult> GetMyTickets([FromQuery] MyTicketListReqDto request)
    {
        var userId = GetUserId();
        var result = await _sellService.GetMyTicketsAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// 티켓 판매 취소
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>취소 결과</returns>
    [HttpDelete("tickets/{ticketId}")]
    [ProducesResponseType(typeof(CancelSellTicketRespDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelTicket(int ticketId)
    {
        var userId = GetUserId();
        var result = await _sellService.CancelTicketAsync(userId, ticketId);
        return Ok(result);
    }
}
