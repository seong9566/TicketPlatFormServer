using Microsoft.AspNetCore.Http;
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
    /// 내 프로필 수정 (닉네임, 자기소개, 프로필 이미지 포함)
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="nickname">닉네임 (nullable)</param>
    /// <param name="bio">자기소개 (nullable)</param>
    /// <param name="profileImage">프로필 이미지 파일 (nullable)</param>
    /// <param name="removeProfileImage">프로필 이미지 삭제 플래그</param>
    /// <returns>업데이트된 프로필 정보</returns>
    Task<UserProfileDto> UpdateMyProfileAsync(int userId, string? nickname, string? bio, IFormFile? profileImage, bool removeProfileImage);

    /// <summary>
    /// 프로필 이미지 URL 갱신 (Signed URL 재발급)
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>새로 발급된 Signed URL (이미지 없으면 null)</returns>
    Task<string?> RefreshProfileImageUrlAsync(int userId);

    /// <summary>
    /// 비밀번호 변경
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="currentPassword">현재 비밀번호</param>
    /// <param name="newPassword">새 비밀번호</param>
    /// <param name="tokenEmail">JWT에서 추출한 이메일 (nullable)</param>
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, string? tokenEmail);

    Task<SocialLoginRespDto> SocialLoginAsync(string providerCode, SocialUserInfoDto socialUserInfo);

    /// <summary>
    /// 전화번호로 아이디(이메일) 찾기
    /// </summary>
    /// <param name="phoneNumber">전화번호</param>
    /// <returns>마스킹된 이메일</returns>
    Task<FindIdResDto> FindIdByPhoneAsync(string phoneNumber);

    /// <summary>
    /// 이메일로 임시 비밀번호 발급
    /// </summary>
    /// <param name="email">계정 이메일</param>
    Task ForgotPasswordAsync(string email);

}
