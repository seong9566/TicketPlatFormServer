using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Reputation;
using TicketPlatFormServer.Services.Reputation;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/reputations")]
[Authorize]
public class ReputationController(IReputationService reputationService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<long>), 201)]
    public async Task<IActionResult> Create([FromBody] CreateReputationReqDto req)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var reputationId = await reputationService.CreateAsync(userId, req);

        return StatusCode(201, new ApiResponse<long>(
            message: "리뷰 작성 완료",
            data: reputationId,
            statusCode: 201
        ));
    }

    [HttpGet("check/{transactionId:long}")]
    [ProducesResponseType(typeof(ApiResponse<ReputationCheckRespDto>), 200)]
    public async Task<IActionResult> Check([FromRoute] long transactionId)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");
        var result = await reputationService.CheckAsync(userId, transactionId);

        return Ok(new ApiResponse<ReputationCheckRespDto>(
            message: "리뷰 가능 여부 조회 성공",
            data: result,
            statusCode: 200
        ));
    }

    [AllowAnonymous]
    [HttpGet("/api/users/{userId:long}/reputations")]
    [ProducesResponseType(typeof(ApiResponse<ReputationListRespDto>), 200)]
    public async Task<IActionResult> GetByUserId([FromRoute] long userId, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var result = await reputationService.GetByUserIdAsync(userId, page, size);

        return Ok(new ApiResponse<ReputationListRespDto>(
            message: "받은 리뷰 목록 조회 성공",
            data: result,
            statusCode: 200
        ));
    }
}
