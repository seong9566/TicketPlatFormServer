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

        public async Task<IActionResult> Sign(RegisterUserReqDto dto)
        {

            var result = _userService.RegisterUser(dto);

            return Ok(new
            {
                message = "회원가입 성공",
                user = result
            });

        }
       

    }
}
