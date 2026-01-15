using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.User;
using TicketPlatFormServer.Services.User;

namespace TicketPlatFormServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// 내 프로필 조회
        /// </summary>
        /// <returns>프로필 정보</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");

            var result = await _userService.GetMyProfileAsync(userId);
            ApiResponse<UserProfileDto> resp = new ApiResponse<UserProfileDto>(
                message: "프로필 조회 성공",
                data: result,
                statusCode: 200
            );

            return Ok(resp);
        }

        /// <summary>
        /// 다른 사용자 프로필 조회
        /// </summary>
        /// <param name="userId">조회할 사용자 ID</param>
        /// <returns>프로필 정보 (공개 정보만)</returns>
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] int userId)
        {
            var result = await _userService.GetUserProfileAsync(userId);
            ApiResponse<UserProfileDto> resp = new ApiResponse<UserProfileDto>(
                message: "프로필 조회 성공",
                data: result,
                statusCode: 200
            );

            return Ok(resp);
        }

        /// <summary>
        /// 내 프로필 수정
        /// </summary>
        /// <param name="dto">수정할 프로필 정보</param>
        /// <returns>수정 성공 메시지</returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileReqDto dto)
        {
            var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");

            await _userService.UpdateMyProfileAsync(userId, dto);
            ApiResponse<object> resp = new ApiResponse<object>(
                message: "프로필 수정 성공",
                data: null,
                statusCode: 200
            );

            return Ok(resp);
        }
    }
}
