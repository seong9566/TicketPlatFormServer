using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Repository.Token;
using TicketPlatFormServer.Services.Token;
using TicketPlatFormServer.Services.User;

namespace TicketPlatFormServer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        // 1. Service 의존성 주입
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        public AuthController(
            IUserService userService,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepo)
        {
            _userService = userService;
            _tokenService = tokenService;
            _refreshTokenRepo = refreshTokenRepo;
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

        /// <summary>
        /// 로그인
        /// </summary>
        /// <param name="dto">이메일과 비밀번호</param>
        /// <returns>로그인된 사용자 정보</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserReqDto dto)
        {
            var result = await _userService.LoginUser(dto);
            ApiResponse<LoginUserRespDto> resp = new ApiResponse<LoginUserRespDto>(
                message: "로그인 성공",
                data: result,
                statusCode: 200
            );

            return Ok(resp);
        }

        /// <summary>
        /// Access Token 갱신
        /// </summary>
        /// <param name="dto">Refresh Token</param>
        /// <returns>새로운 Access Token 및 Refresh Token</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenReqDto dto)
        {
            // 1. Refresh Token 조회 (User, Role, Provider 포함)
            var refreshToken = await _refreshTokenRepo.GetRefreshTokenAsync(dto.RefreshToken);
            if (refreshToken == null)
            {
                throw new AppException(message: "유효하지 않은 Refresh Token입니다.", statusCode: HttpStatusCode.Unauthorized);
            }

            // 2. Refresh Token 유효성 검증 (만료 여부 + 무효화 여부)
            var isValid = await _refreshTokenRepo.IsTokenValidAsync(dto.RefreshToken);
            if (!isValid)
            {
                throw new AppException(message: "만료되었거나 무효화된 Refresh Token입니다.", statusCode: HttpStatusCode.Unauthorized);
            }

            // 3. 새로운 Access Token + Refresh Token 생성
            var newTokenResponse = await _tokenService.GenerateTokensAsync(refreshToken.User, 7);

            // 4. 이전 Refresh Token 무효화 (Token Rotation)
            await _refreshTokenRepo.RevokeRefreshTokenAsync(dto.RefreshToken, newTokenResponse.RefreshToken);

            // 5. 새로운 Refresh Token DB 저장
            var newRefreshToken = new DBModel.RefreshToken
            {
                UserId = refreshToken.UserId,
                Token = newTokenResponse.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };
            await _refreshTokenRepo.SaveRefreshTokenAsync(newRefreshToken);

            // 6. 응답
            ApiResponse<TokenResponseDto> resp = new ApiResponse<TokenResponseDto>(
                message: "Token 갱신 성공",
                data: newTokenResponse,
                statusCode: 200
            );

            return Ok(resp);
        }

        /// <summary>
        /// 로그아웃 (Refresh Token 무효화)
        /// </summary>
        /// <param name="dto">Refresh Token</param>
        /// <returns>로그아웃 성공 메시지</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenReqDto dto)
        {
            // 1. Refresh Token 무효화
            var revoked = await _refreshTokenRepo.RevokeRefreshTokenAsync(dto.RefreshToken);
            if (!revoked)
            {
                throw new AppException(message: "유효하지 않은 Refresh Token입니다.", statusCode: HttpStatusCode.BadRequest);
            }

            // 2. 응답
            ApiResponse<object> resp = new ApiResponse<object>(
                message: "로그아웃 성공",
                data: null,
                statusCode: 200
            );

            return Ok(resp);
        }

    }
}
