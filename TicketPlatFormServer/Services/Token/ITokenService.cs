using TicketPlatFormServer.DTO;
using UserEntity = TicketPlatFormServer.DBModel.User;

namespace TicketPlatFormServer.Services.Token;

/// <summary>
/// Token 생성 및 검증 Service 인터페이스
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Access Token 및 Refresh Token 생성
    /// </summary>
    /// <param name="user">사용자 정보</param>
    /// <param name="refreshTokenExpirationDays">Refresh Token 만료 일수</param>
    /// <returns>Token 응답 DTO</returns>
    Task<TokenResponseDto> GenerateTokensAsync(UserEntity user, int refreshTokenExpirationDays);

    /// <summary>
    /// Access Token 생성
    /// </summary>
    /// <param name="user">사용자 정보</param>
    /// <returns>JWT Access Token</returns>
    string GenerateAccessToken(UserEntity user);

    /// <summary>
    /// Refresh Token 생성 (UUID)
    /// </summary>
    /// <returns>Refresh Token (UUID 문자열)</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Token 유효성 검증
    /// </summary>
    /// <param name="token">검증할 Access Token</param>
    /// <returns>유효하면 true, 아니면 false</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Token에서 UserId 추출
    /// </summary>
    /// <param name="token">Access Token</param>
    /// <returns>UserId (추출 실패 시 null)</returns>
    int? GetUserIdFromToken(string token);
}
