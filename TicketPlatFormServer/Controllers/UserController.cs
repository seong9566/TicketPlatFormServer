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
        [HttpGet("profile/user")]
        public async Task<IActionResult> GetUserProfile([FromQuery] int userId)
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
        /// 내 프로필 수정 (닉네임, 자기소개, 프로필 이미지 포함)
        /// </summary>
        /// <param name="nickname">닉네임 (선택)</param>
        /// <param name="bio">자기소개 (선택)</param>
        /// <param name="profileImage">프로필 이미지 파일 (선택)</param>
        /// <param name="removeProfileImage">프로필 이미지 삭제 플래그 (기본값: false)</param>
        /// <returns>업데이트된 프로필 정보</returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromForm] string? nickname,
            [FromForm] string? bio,
            [FromForm] IFormFile? profileImage,
            [FromForm] bool removeProfileImage = false)
        {
            var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");

            var result = await _userService.UpdateMyProfileAsync(userId, nickname, bio, profileImage, removeProfileImage);
            ApiResponse<UserProfileDto> resp = new ApiResponse<UserProfileDto>(
                message: "프로필 수정 성공",
                data: result,
                statusCode: 200
            );

            return Ok(resp);
        }

        /// <summary>
        /// 프로필 이미지 URL 갱신 (Signed URL 재발급)
        /// </summary>
        /// <param name="reqDto">갱신 요청 DTO (userId가 null이면 본인)</param>
        /// <returns>새로 발급된 Signed URL</returns>
        [HttpPost("profile/image-refresh")]
        public async Task<IActionResult> RefreshProfileImageUrl([FromBody] ProfileImageRefreshReqDto? reqDto)
        {
            // userId가 null이면 본인의 프로필 이미지 URL 갱신
            int targetUserId = reqDto?.UserId 
                ?? User.GetUserId() 
                ?? throw new UnauthorizedAccessException("사용자 인증 정보가 유효하지 않습니다.");

            var newUrl = await _userService.RefreshProfileImageUrlAsync(targetUserId);

            // 프로필 이미지가 없는 경우
            if (newUrl == null)
            {
                ApiResponse<ProfileImageRefreshRespDto> noImageResp = new ApiResponse<ProfileImageRefreshRespDto>(
                    message: "프로필 이미지가 없습니다.",
                    data: new ProfileImageRefreshRespDto { ProfileImageUrl = null },
                    statusCode: 200
                );
                return Ok(noImageResp);
            }

            ApiResponse<ProfileImageRefreshRespDto> resp = new ApiResponse<ProfileImageRefreshRespDto>(
                message: "이미지 URL 갱신 성공",
                data: new ProfileImageRefreshRespDto { ProfileImageUrl = newUrl },
                statusCode: 200
            );

            return Ok(resp);
        }
    }
}
