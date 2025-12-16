
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.Repository;

public class UserRepository :IUserRepository
{
    private readonly TicketContext _db;

    public UserRepository(TicketContext db)
    {
        // Context 의존성 주입
        _db = db;

    }
    
    /// <summary>
    /// 이메일로 사용자 찾기 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User?> GetByEmail(string email)
    {
        var user = await _db.Users
            .Include(u => u.Provider)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Email == email && x.IsDeleted == false);
        return user;
    }

    /// <summary>
    ///  사용자 추가
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<User> Sign(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// 마지막 로그인 시간 업데이트
    /// </summary>
    /// <param name="userId"></param>
    public async Task UpdateLastLoginAt(long userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    ///  Provider코드로 사용자 찾기 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<AuthProvider?> GetProviderByCode(string code)
    {
        return await _db.AuthProviders.FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
    }

    /// <summary>
    ///  Role 값으로 사용자 찾기 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<AuthRole?> GetRoleByCode(string code)
    {
        return await _db.AuthRoles.FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
    }
} 