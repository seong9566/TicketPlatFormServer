using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
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
    /// 판매 가능한 카테고리 목록 조회
    /// </summary>
    /// <returns>카테고리 목록</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryRespDto>>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _sellService.GetCategoriesAsync();
        var resp = new ApiResponse<List<CategoryRespDto>>(
            message: "카테고리 목록 조회 성공",
            data: categories,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 카테고리별 공연 목록 조회 (페이징)
    /// </summary>
    /// <param name="request">조회 조건</param>
    /// <returns>공연 목록 (페이징)</returns>
    [HttpGet("events")]
    [ProducesResponseType(typeof(ApiResponse<SellEventListRespDto>), 200)]
    public async Task<IActionResult> GetEvents([FromQuery] SellEventListReqDto request)
    {
        var events = await _sellService.GetEventsAsync(request);
        var resp = new ApiResponse<SellEventListRespDto>(
            message: "공연 목록 조회 성공",
            data: events,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 특정 공연의 일정 목록 조회
    /// </summary>
    /// <param name="eventId">공연 ID</param>
    /// <returns>일정 목록</returns>
    [HttpGet("events/schedules")]
    [ProducesResponseType(typeof(ApiResponse<EventScheduleRespDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEventSchedules([FromQuery] int eventId)
    {
        var schedules = await _sellService.GetEventSchedulesAsync(eventId);
        var resp = new ApiResponse<EventScheduleRespDto>(
            message: "일정 목록 조회 성공",
            data: schedules,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 특정 공연의 좌석 옵션 조회
    /// </summary>
    /// <param name="eventId">공연 ID</param>
    /// <returns>좌석 옵션 목록</returns>
    [HttpGet("events/seat-options")]
    [ProducesResponseType(typeof(ApiResponse<SeatOptionRespDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSeatOptions([FromQuery] int eventId)
    {
        var options = await _sellService.GetSeatOptionsAsync(eventId);
        var resp = new ApiResponse<SeatOptionRespDto>(
            message: "좌석 옵션 조회 성공",
            data: options,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 공연 좌석 정가 조회
    /// </summary>
    /// <param name="eventId">공연 ID</param>
    /// <param name="request">정가 조회 조건</param>
    /// <returns>정가 (없으면 null)</returns>
    [HttpGet("events/original-price")]
    [ProducesResponseType(typeof(ApiResponse<int?>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOriginalPrice([FromQuery] GetOriginalPriceReqDto request)
    {
        var price = await _sellService.GetOriginalPriceAsync(request);
        var resp = new ApiResponse<int?>(
            message: "정가 조회 성공",
            data: price,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 티켓 판매 등록
    /// </summary>
    /// <param name="request">티켓 판매 등록 정보</param>
    /// <returns>등록 결과</returns>
    [HttpPost("tickets")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<CreateSellTicketRespDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateTicket([FromForm] CreateSellTicketReqDto request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await _sellService.CreateTicketAsync(userId, request);
        var resp = new ApiResponse<CreateSellTicketRespDto>(
            message: "티켓 판매 등록 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 내 판매 티켓 목록 조회
    /// </summary>
    /// <param name="request">조회 조건</param>
    /// <returns>내 티켓 목록 (페이징)</returns>
    [HttpGet("my-tickets")]
    [ProducesResponseType(typeof(ApiResponse<MyTicketListRespDto>), 200)]
    public async Task<IActionResult> GetMyTickets([FromQuery] MyTicketListReqDto request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await _sellService.GetMyTicketsAsync(userId, request);
        var resp = new ApiResponse<MyTicketListRespDto>(
            message: "내 판매 티켓 목록 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 티켓 판매 취소
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>취소 결과</returns>
    [HttpDelete("tickets")]
    [ProducesResponseType(typeof(ApiResponse<CancelSellTicketRespDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelTicket([FromQuery] int ticketId)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await _sellService.CancelTicketAsync(userId, ticketId);
        var resp = new ApiResponse<CancelSellTicketRespDto>(
            message: "티켓 판매 취소 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 티켓 이미지 URL 재발급
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>갱신된 이미지 URL 목록</returns>
    [HttpGet("tickets/images/refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTicketImageUrlRespDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RefreshTicketImageUrls([FromQuery] int ticketId)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await _sellService.RefreshTicketImageUrlsAsync(ticketId, userId);
        var resp = new ApiResponse<RefreshTicketImageUrlRespDto>(
            message: "티켓 이미지 URL 재발급 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 티켓 특이사항 목록 조회 (판매 등록 시 선택 가능한 특이사항)
    /// </summary>
    /// <returns>특이사항 목록</returns>
    [HttpGet("features")]
    [ProducesResponseType(typeof(ApiResponse<List<TicketFeatureRespDto>>), 200)]
    public async Task<IActionResult> GetTicketFeatures()
    {
        var features = await _sellService.GetTicketFeaturesAsync();
        var resp = new ApiResponse<List<TicketFeatureRespDto>>(
            message: "특이사항 목록 조회 성공",
            data: features,
            statusCode: 200
        );
        return Ok(resp);
    }
}

