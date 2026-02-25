using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Settlement;
using TicketPlatFormServer.Services.Settlements;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/settlement")]
[Authorize]
public class SettlementController(ISettlementService settlementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMySettlements()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await settlementService.GetMySettlementsAsync(userId.Value);
        return Ok(new ApiResponse<SettlementListResponseDto>("정산 목록 조회가 완료되었습니다.", data, 200));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetSettlementById([FromRoute] long id)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await settlementService.GetSettlementByIdAsync(id, userId.Value);
        if (data == null)
        {
            throw new AppException("정산 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        return Ok(new ApiResponse<SettlementResponseDto>("정산 상세 조회가 완료되었습니다.", data, 200));
    }
}
