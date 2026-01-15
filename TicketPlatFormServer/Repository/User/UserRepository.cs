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
    /// ID로 사용자 조회
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>User 엔티티 (없으면 null)</returns>
    public async Task<User?> GetByIdAsync(int userId)
    {
        var user = await db.Users
            .Include(u => u.Provider)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsDeleted == false);
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

    /// <summary>
    /// 사용자 및 기본 프로필 생성 (원자적 트랜잭션)
    /// </summary>
    /// <param name="user">User 엔티티</param>
    /// <param name="profile">UserProfile 엔티티</param>
    /// <returns>생성된 User (Id 포함)</returns>
    public async Task<User> CreateUserWithProfile(User user, UserProfile profile)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        // EF Core에서 MySQL용 Retry Strategy가 활성화된 상태에서 BeginTransactionAsync()를 직접 호출하면 충돌이 발생
        // MySQL 연결 장애 시 자동 재시도 로직과 수동 트랜잭션이 함께 사용될 수 없기 때문
        // strategy 추가
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. User 저장
                db.Users.Add(user);
                await db.SaveChangesAsync();

                // 2. UserProfile 저장 (UserId 자동 설정)
                profile.UserId = user.Id;
                db.UserProfiles.Add(profile);
                await db.SaveChangesAsync();

                // 3. 트랜잭션 커밋
                await transaction.CommitAsync();
                return user;
            }
            catch
            {
                // 4. 에러 발생 시 롤백
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// 닉네임 중복 확인
    /// </summary>
    /// <param name="nickname">확인할 닉네임</param>
    /// <returns>이미 존재하면 true, 아니면 false</returns>
    public async Task<bool> IsNicknameExistsAsync(string nickname)
    {
        return await db.UserProfiles.AnyAsync(up => up.Nickname == nickname);
    }

    /// <summary>
    /// 사용자 프로필 조회
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>UserProfile 엔티티 (없으면 null)</returns>
    public async Task<UserProfile?> GetUserProfileByIdAsync(int userId)
    {
        return await db.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
    }

    /// <summary>
    /// 사용자 프로필 업데이트
    /// </summary>
    /// <param name="profile">업데이트할 UserProfile 엔티티</param>
    public async Task UpdateUserProfileAsync(UserProfile profile)
    {
        db.UserProfiles.Update(profile);
        await db.SaveChangesAsync();
    }
} 