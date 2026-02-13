using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Dispute;
using TicketPlatFormServer.Services.Dispute;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/disputes")]
[Authorize]
public class DisputeController(IDisputeService disputeService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DisputeSummaryRespDto>), 201)]
    public async Task<IActionResult> CreateDispute([FromBody] CreateDisputeReqDto req)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await disputeService.CreateDisputeAsync(userId, req);

        return StatusCode(201, new ApiResponse<DisputeSummaryRespDto>(
            message: "신고 접수 완료",
            data: result,
            statusCode: 201
        ));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DisputeListRespDto>), 200)]
    public async Task<IActionResult> GetMyDisputes([FromQuery] string? cursor = null, [FromQuery] int? limit = 20)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await disputeService.GetMyDisputesAsync(userId, cursor, limit);

        return Ok(new ApiResponse<DisputeListRespDto>(
            message: "신고 목록 조회 성공",
            data: result,
            statusCode: 200
        ));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<DisputeDetailRespDto>), 200)]
    public async Task<IActionResult> GetDisputeDetail([FromRoute] long id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await disputeService.GetDisputeDetailAsync(userId, id);

        return Ok(new ApiResponse<DisputeDetailRespDto>(
            message: "신고 상세 조회 성공",
            data: result,
            statusCode: 200
        ));
    }

    [HttpPost("{id:long}/evidence")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<AddDisputeEvidenceRespDto>), 201)]
    public async Task<IActionResult> AddEvidence([FromRoute] long id, [FromForm] AddDisputeEvidenceReqDto req)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await disputeService.AddEvidenceAsync(userId, id, req);

        return StatusCode(201, new ApiResponse<AddDisputeEvidenceRespDto>(
            message: "증거 첨부 완료",
            data: result,
            statusCode: 201
        ));
    }

    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<CancelDisputeRespDto>), 200)]
    public async Task<IActionResult> CancelDispute([FromRoute] long id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await disputeService.CancelDisputeAsync(userId, id);

        return Ok(new ApiResponse<CancelDisputeRespDto>(
            message: "신고 취소 완료",
            data: result,
            statusCode: 200
        ));
    }
}
