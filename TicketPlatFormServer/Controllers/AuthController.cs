using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Services.User;

namespace TicketPlatFormServer.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : Controller
    {
        // 1. Service 의존성 주입
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // [FromBody] : 클라이언트가 보내는 JSON을 받아 DTO로 자동 변환해주는 어노테이션
        // 실무에서 많이 쓰는 표준 방식이다.
        // 그외 FromQuery, FromRoute 방식.
        // FromQuery : ([FromQuery] string email) /auth/sign?email=test@email.com
        // FromRoute : ([FromRoute] int userId) /auth/sign/10
        [HttpPost("sign")]
        public async Task<IActionResult> Sign([FromBody] RegisterUserReqDto dto)
        {

            var result = await _userService.RegisterUser(dto);
            ApiResponse<RegisterUserRespDto> resp = new ApiResponse<RegisterUserRespDto>(
                message: "회원가입 성공",
                data: result,
                statusCode: 200
            );

            return Ok(resp);

        }
       

    }
}
