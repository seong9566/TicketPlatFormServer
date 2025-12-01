
using TicketPlatFormServer.DBModel;

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
}