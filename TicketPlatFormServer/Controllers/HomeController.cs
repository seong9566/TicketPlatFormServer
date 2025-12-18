using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Home;
using TicketPlatFormServer.Services.Home;

namespace TicketPlatFormServer.Controllers;

// 컨트롤러 단에서 try문을 걸지 않으니 전체적인 에러를 내가 디버깅으로 확인이 안됌..
[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    /// <summary>
    /// 홈 화면 데이터 조회 (인기 티켓 + 추천 이벤트)
    /// </summary>
    /// <param name="userId">사용자 ID (선택, 로그인 시 개인화 추천)</param>
    /// <returns>홈 화면 데이터</returns>
    [HttpGet]
    public async Task<IActionResult> GetHomeData([FromQuery] int? userId)
    {
        var result = await _homeService.GetHomeData(userId);
        var resp = new ApiResponse<HomeRespDto>(
            message: "홈 화면 데이터 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }
}

