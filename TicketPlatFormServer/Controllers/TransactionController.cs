using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Transaction;
using TicketPlatFormServer.Services.Transaction;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionController(ITransactionService transactionService) : ControllerBase
{
    [HttpGet("purchases")]
    [ProducesResponseType(typeof(ApiResponse<TransactionHistoryRespDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetPurchaseHistory(
        [FromQuery] string? status = null,
        [FromQuery] string? period = "all",
        [FromQuery] string? sortBy = "latest",
        [FromQuery] string? cursor = null,
        [FromQuery] int? limit = 20)
    {
        var userId = User.GetUserId()
            ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");

        var result = await transactionService.GetPurchaseHistoryAsync(
            userId,
            status,
            period,
            sortBy,
            cursor,
            limit);

        return Ok(new ApiResponse<TransactionHistoryRespDto>(
            message: "구매 내역 조회 성공",
            data: result,
            statusCode: 200
        ));
    }

}
