using System.Data;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Users;

/// <summary>
/// 사용자 Repository 구현체 (Primary Constructor 사용)
/// </summary>
public class UserRepository(TicketContext db, IDbConnection dapper) : IUserRepository
{
    
    /// <summary>
    /// 이메일로 사용자 찾기 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User?> GetByEmail(string email)
    {
        var user = await db.Users
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
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// 마지막 로그인 시간 업데이트
    /// </summary>
    /// <param name="userId"></param>
    public async Task UpdateLastLoginAt(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    ///  Provider코드로 사용자 찾기
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<AuthProvider?> GetProviderByCode(string code)
    {
        return await db.AuthProviders.FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
    }

    /// <summary>
    ///  Role 값으로 사용자 찾기
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<AuthRole?> GetRoleByCode(string code)
    {
        return await db.AuthRoles.FirstOrDefaultAsync(x => x.Code == code && x.IsActive == true);
    }
} 