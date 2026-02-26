using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.BankAccount;
using TicketPlatFormServer.Services.BankAccount;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/bank-account")]
[Authorize]
public class BankAccountController(IBankAccountService bankAccountService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterBankAccount([FromBody] RegisterBankAccountRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await bankAccountService.RegisterBankAccountAsync(request, userId.Value);
        return StatusCode(201, new ApiResponse<BankAccountResponseDto>("계좌 등록이 완료되었습니다.", data, 201));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyBankAccount()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        var data = await bankAccountService.GetMyBankAccountAsync(userId.Value);
        return Ok(new ApiResponse<BankAccountResponseDto?>("계좌 조회가 완료되었습니다.", data, 200));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBankAccount()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);
        }

        await bankAccountService.DeleteBankAccountAsync(userId.Value);
        return Ok(new ApiResponse<object?>("계좌 삭제가 완료되었습니다.", null, 200));
    }
}
