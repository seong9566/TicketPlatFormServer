using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Users;

/// <summary>
/// Repository는 DB와 1:1로 맞닿아 있는 계층이다.
/// 그래서 파라미터 값이 DTO가 되면 안됀다.
/// Service에서 DTO -> DBModel로 변경 -> Repository -> DB 순서가 되어야한다.
/// </summary>
public interface IUserRepository
{

    /// <summary>
    /// email로 User값을 받아옴.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetByEmail(string email);

    /// <summary>
    /// 회원가입
    /// </summary>
    /// DB에서 가져온 객체가 되어야함.
    /// <param name="user"></param>
    Task<User> Sign(User user);

    /// <summary>
    /// 마지막 로그인 시간 업데이트
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    Task UpdateLastLoginAt(int userId);

    /// <summary>
    /// Provider Code로 Provider 조회
    /// </summary>
    /// <param name="code">Provider Code</param>
    /// <returns></returns>
    Task<AuthProvider?> GetProviderByCode(string code);

    /// <summary>
    /// Role Code로 Role 조회
    /// </summary>
    /// <param name="code">Role Code</param>
    /// <returns></returns>
    Task<AuthRole?> GetRoleByCode(string code);

    /// <summary>
    /// 사용자 및 기본 프로필 생성 (원자적 트랜잭션)
    /// </summary>
    /// <param name="user">User 엔티티</param>
    /// <param name="profile">UserProfile 엔티티</param>
    /// <returns>생성된 User (Id 포함)</returns>
    Task<User> CreateUserWithProfile(User user, UserProfile profile);

    /// <summary>
    /// 닉네임 중복 확인
    /// </summary>
    /// <param name="nickname">확인할 닉네임</param>
    /// <returns>이미 존재하면 true, 아니면 false</returns>
    Task<bool> IsNicknameExistsAsync(string nickname);

    /// <summary>
    /// 사용자 프로필 조회
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>UserProfile 엔티티 (없으면 null)</returns>
    Task<UserProfile?> GetUserProfileByIdAsync(int userId);

    /// <summary>
    /// 사용자 프로필 업데이트
    /// </summary>
    /// <param name="profile">업데이트할 UserProfile 엔티티</param>
    Task UpdateUserProfileAsync(UserProfile profile);
}