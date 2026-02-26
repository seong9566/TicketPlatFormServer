using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.Services.Withdrawal;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/balance")]
[Authorize]
public class BalanceController(IWithdrawalService withdrawalService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyBalance()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await withdrawalService.GetBalanceAsync(userId.Value);
        return Ok(new ApiResponse<BalanceResponseDto>("잔고 조회가 완료되었습니다.", data, 200));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetBalanceHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await withdrawalService.GetBalanceHistoryAsync(userId.Value, page, pageSize);
        return Ok(new ApiResponse<BalanceHistoryResponseDto>("잔고 내역 조회가 완료되었습니다.", data, 200));
    }
}
