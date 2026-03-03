using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Dispute;
using TicketPlatFormServer.Services.Dispute;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/admin/disputes")]
[Authorize]
public class AdminDisputeController(IDisputeService disputeService) : ControllerBase
{
    [HttpPost("{id:long}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<AdminResolveDisputeRespDto>), 200)]
    public async Task<IActionResult> ResolveDispute(
        [FromRoute] long id,
        [FromBody] AdminResolveDisputeReqDto req)
    {
        var adminRole = User.GetRole();
        if (adminRole != "admin")
        {
            throw new AppException("관리자 권한이 필요합니다.", HttpStatusCode.Forbidden);
        }

        var adminUserId = User.GetUserId() ?? throw new AppException("사용자 인증 정보가 유효하지 않습니다.", HttpStatusCode.Unauthorized);
        var result = await disputeService.ResolveDisputeAsync(adminUserId, id, req);

        return Ok(new ApiResponse<AdminResolveDisputeRespDto>(
            message: "분쟁 해결 완료",
            data: result,
            statusCode: 200
        ));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AdminDisputeListRespDto>), 200)]
    public async Task<IActionResult> GetAllDisputes(
        [FromQuery] string? statusFilter = null,
        [FromQuery] string? cursor = null,
        [FromQuery] int? limit = 20)
    {
        var adminRole = User.GetRole();
        if (adminRole != "admin")
        {
            throw new AppException("관리자 권한이 필요합니다.", HttpStatusCode.Forbidden);
        }

        var result = await disputeService.GetAllDisputesAsync(statusFilter, cursor, limit);

        return Ok(new ApiResponse<AdminDisputeListRespDto>(
            message: "분쟁 목록 조회 성공",
            data: result,
            statusCode: 200
        ));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<DisputeDetailRespDto>), 200)]
    public async Task<IActionResult> GetDisputeDetailForAdmin([FromRoute] long id)
    {
        var adminRole = User.GetRole();
        if (adminRole != "admin")
        {
            throw new AppException("관리자 권한이 필요합니다.", HttpStatusCode.Forbidden);
        }

        var result = await disputeService.GetDisputeDetailForAdminAsync(id);

        return Ok(new ApiResponse<DisputeDetailRespDto>(
            message: "분쟁 상세 조회 성공",
            data: result,
            statusCode: 200
        ));
    }
}
