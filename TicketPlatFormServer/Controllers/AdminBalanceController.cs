using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Admin;
using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.DTO.Withdrawal;
using TicketPlatFormServer.Services.Balance;
using TicketPlatFormServer.Services.Withdrawal;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/admin/balance")]
[Authorize]
public class AdminBalanceController(IBalanceService balanceService, IWithdrawalService withdrawalService) : ControllerBase
{
    [HttpPost("{userId:int}/adjust")]
    public async Task<IActionResult> AdjustBalance(
        [FromRoute] int userId,
        [FromBody] AdminAdjustBalanceRequestDto request)
    {
        var adminRole = User.GetRole();
        if (adminRole != "admin")
        {
            throw new AppException("관리자 권한이 필요합니다.", HttpStatusCode.Forbidden);
        }

        var data = await balanceService.AdminAdjustBalanceAsync(userId, request.Amount, request.Reason);
        return Ok(new ApiResponse<BalanceResponseDto>("잔고 조정이 완료되었습니다.", data, 200));
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserBalance([FromRoute] int userId)
    {
        var adminRole = User.GetRole();
        if (adminRole != "admin")
        {
            throw new AppException("관리자 권한이 필요합니다.", HttpStatusCode.Forbidden);
        }

        var data = await withdrawalService.GetBalanceAsync(userId);
        return Ok(new ApiResponse<BalanceResponseDto>("잔고 조회가 완료되었습니다.", data, 200));
    }

    [HttpGet("{userId:int}/withdrawals")]
    public async Task<IActionResult> GetUserWithdrawals(
        [FromRoute] int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var adminRole = User.GetRole();
        if (adminRole != "admin")
        {
            throw new AppException("관리자 권한이 필요합니다.", HttpStatusCode.Forbidden);
        }

        var data = await withdrawalService.GetWithdrawalHistoryAsync(userId, page, pageSize);
        return Ok(new ApiResponse<WithdrawalListResponseDto>("출금 내역 조회가 완료되었습니다.", data, 200));
    }
}
