using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Settlement;
using TicketPlatFormServer.Services.Settlements;

namespace TicketPlatFormServer.Controllers;

/// <summary>
/// 정산 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/settlement")]
[Authorize]
public class SettlementController(ISettlementService settlementService) : ControllerBase
{
    /// <summary>
    /// 내 정산 목록 조회 (페이징 + 상태 필터)
    /// </summary>
    /// <param name="page">페이지 번호 (기본값: 1)</param>
    /// <param name="pageSize">페이지 크기 (기본값: 20)</param>
    /// <param name="status">상태 필터 (pending, processing, completed, failed 등)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SettlementListResponseDto>), 200)]
    public async Task<IActionResult> GetMySettlementsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var userId = User.GetUserId() ?? throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        var data = await settlementService.GetBySellerAsync(userId, page, pageSize, status);
        return Ok(new ApiResponse<SettlementListResponseDto>("정산 목록 조회가 완료되었습니다.", data, 200));
    }

    /// <summary>
    /// 정산 상세 조회
    /// </summary>
    /// <param name="id">정산 ID</param>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<SettlementDetailRespDto>), 200)]
    public async Task<IActionResult> GetSettlementByIdAsync([FromRoute] long id)
    {
        var userId = User.GetUserId() ?? throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        var data = await settlementService.GetDetailAsync(id, userId);
        return Ok(new ApiResponse<SettlementDetailRespDto>("정산 상세 조회가 완료되었습니다.", data, 200));
    }
}
