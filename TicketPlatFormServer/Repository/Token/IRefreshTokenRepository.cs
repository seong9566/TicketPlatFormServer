using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Token;

/// <summary>
/// Refresh Token Repository 인터페이스
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Refresh Token 저장
    /// </summary>
    /// <param name="refreshToken">저장할 Refresh Token</param>
    /// <returns>저장된 Refresh Token</returns>
    Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken);

    /// <summary>
    /// Refresh Token 조회 (Token 문자열로)
    /// </summary>
    /// <param name="token">조회할 Token 문자열</param>
    /// <returns>Refresh Token 엔티티 (없으면 null)</returns>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    /// <summary>
    /// Refresh Token 무효화
    /// </summary>
    /// <param name="token">무효화할 Token 문자열</param>
    /// <param name="replacedByToken">대체하는 새 Token (Token Rotation 시)</param>
    /// <returns>무효화 성공 여부</returns>
    Task<bool> RevokeRefreshTokenAsync(string token, string? replacedByToken = null);

    /// <summary>
    /// 사용자의 모든 Refresh Token 무효화 (로그아웃 전체 세션)
    /// </summary>
    /// <param name="userId">사용자 ID</param>
    /// <returns>무효화된 Token 개수</returns>
    Task<int> RevokeAllUserTokensAsync(int userId);

    /// <summary>
    /// 만료된 Refresh Token 삭제 (정리 작업)
    /// </summary>
    /// <returns>삭제된 Token 개수</returns>
    Task<int> RemoveExpiredTokensAsync();

    /// <summary>
    /// Refresh Token 유효성 확인 (만료 여부 + 무효화 여부)
    /// </summary>
    /// <param name="token">확인할 Token 문자열</param>
    /// <returns>유효하면 true, 아니면 false</returns>
    Task<bool> IsTokenValidAsync(string token);
}
