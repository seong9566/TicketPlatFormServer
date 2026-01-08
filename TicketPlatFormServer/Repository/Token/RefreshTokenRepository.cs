using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Token;

/// <summary>
/// Refresh Token Repository 구현체 (Primary Constructor 패턴)
/// </summary>
public class RefreshTokenRepository(
    TicketContext db,
    ILogger<RefreshTokenRepository> logger) : IRefreshTokenRepository
{
    public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        refreshToken.CreatedAt = DateTime.UtcNow;
        refreshToken.IsRevoked = false;

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "[RefreshTokenRepository.SaveRefreshTokenAsync] Refresh Token 저장 완료 | UserId: {UserId}, TokenId: {TokenId}",
            refreshToken.UserId, refreshToken.Id
        );

        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var refreshToken = await db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .Include(rt => rt.User)
                .ThenInclude(u => u.Provider)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        return refreshToken;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string? replacedByToken = null)
    {
        var refreshToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = replacedByToken;

        await db.SaveChangesAsync();

        logger.LogInformation(
            "[RefreshTokenRepository.RevokeRefreshTokenAsync] Refresh Token 무효화 완료 | UserId: {UserId}, TokenId: {TokenId}",
            refreshToken.UserId, refreshToken.Id
        );

        return true;
    }

    public async Task<int> RevokeAllUserTokensAsync(int userId)
    {
        var userTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsRevoked != true)
            .ToListAsync();

        var revokedCount = 0;
        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            revokedCount++;
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "[RefreshTokenRepository.RevokeAllUserTokensAsync] 사용자의 모든 Token 무효화 완료 | UserId: {UserId}, RevokedCount: {Count}",
            userId, revokedCount
        );

        return revokedCount;
    }

    public async Task<int> RemoveExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await db.RefreshTokens
            .Where(rt => rt.ExpiryDate < now)
            .ToListAsync();

        var deletedCount = expiredTokens.Count;
        db.RefreshTokens.RemoveRange(expiredTokens);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "[RefreshTokenRepository.RemoveExpiredTokensAsync] 만료된 Token 삭제 완료 | DeletedCount: {Count}",
            deletedCount
        );

        return deletedCount;
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var now = DateTime.UtcNow;
        var refreshToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return false;

        // 무효화되었거나 만료된 경우 false
        if (refreshToken.IsRevoked == true || refreshToken.ExpiryDate < now)
            return false;

        return true;
    }
}
