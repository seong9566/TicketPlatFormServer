using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.User;

namespace TicketPlatFormServer.Services.User;

public interface IUserService
{
    /// <summary>
    /// 회원가입
    /// </summary>
    Task<RegisterUserRespDto> RegisterUser(RegisterUserReqDto dto);

    /// <summary>
    /// 로그인
    /// </summary>
    Task<LoginUserRespDto> LoginUser(LoginUserReqDto dto);

    /// <summary>
    /// 내 프로필 조회
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>프로필 정보</returns>
    Task<UserProfileDto> GetMyProfileAsync(int userId);

    /// <summary>
    /// 다른 사용자 프로필 조회 (공개 정보만)
    /// </summary>
    /// <param name="userId">조회할 사용자 ID</param>
    /// <returns>프로필 정보 (공개 정보만)</returns>
    Task<UserProfileDto> GetUserProfileAsync(int userId);

    /// <summary>
    /// 내 프로필 수정
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="dto">수정할 프로필 정보</param>
    Task UpdateMyProfileAsync(int userId, UpdateUserProfileReqDto dto);
}