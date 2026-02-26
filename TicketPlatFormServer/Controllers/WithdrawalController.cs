using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Withdrawal;
using TicketPlatFormServer.Services.Withdrawal;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/withdrawal")]
[Authorize]
public class WithdrawalController(IWithdrawalService withdrawalService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RequestWithdrawal(
        [FromBody] WithdrawalRequestDto request,
        [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await withdrawalService.RequestWithdrawalAsync(userId.Value, request, idempotencyKey);
        return Ok(new ApiResponse<WithdrawalResponseDto>("출금 요청이 완료되었습니다.", data, 200));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetWithdrawalHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await withdrawalService.GetWithdrawalHistoryAsync(userId.Value, page, pageSize);
        return Ok(new ApiResponse<WithdrawalListResponseDto>("출금 내역 조회가 완료되었습니다.", data, 200));
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> CancelWithdrawal([FromRoute] long id)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await withdrawalService.CancelWithdrawalAsync(userId.Value, id);
        return Ok(new ApiResponse<WithdrawalResponseDto>("출금 취소가 완료되었습니다.", data, 200));
    }
}
