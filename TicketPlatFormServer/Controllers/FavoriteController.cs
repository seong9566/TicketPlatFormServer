using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Favorite;
using TicketPlatFormServer.Services.Favorite;

namespace TicketPlatFormServer.Controllers;

/// <summary>
/// 찜 관련 컨트롤러
/// </summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// 티켓 찜 토글 (추가/삭제)
    /// </summary>
    /// <param name="req">티켓 찜 요청 DTO</param>
    /// <returns>찜 상태 결과</returns>
    [HttpPost("tickets")]
    public async Task<IActionResult> ToggleTicketFavorite([FromBody] FavoriteToggleReqDto req)
    {
        // Claims에서 userId 추출
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        // DTO에 userId 설정
        req.UserId = userId.Value;

        var result = await _favoriteService.ToggleTicketFavorite(req);
        var resp = new ApiResponse<FavoriteToggleRespDto>(
            message: result.IsFavorited ? "티켓 찜 추가 완료" : "티켓 찜 해제 완료",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 사용자가 찜한 티켓 목록 조회
    /// </summary>
    /// <returns>찜한 티켓 목록</returns>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetFavoriteTickets()
    {
        // Claims에서 userId 추출
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await _favoriteService.GetFavoriteTicketsByUserId(userId.Value);
        var resp = new ApiResponse<List<FavoriteTicketListRespDto>>(
            message: "찜한 티켓 목록 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }
}
