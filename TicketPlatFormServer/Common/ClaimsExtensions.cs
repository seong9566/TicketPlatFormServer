using System.Security.Claims;

namespace TicketPlatFormServer.Common;

/// <summary>
/// ClaimsPrincipal 확장 메서드 (JWT Claims 추출)
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// ClaimsPrincipal에서 UserId 추출
    /// </summary>
    /// <param name="user">ClaimsPrincipal</param>
    /// <returns>UserId (추출 실패 시 null)</returns>
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("userId");
        return int.TryParse(claim?.Value, out var id) ? id : null;
    }

    /// <summary>
    /// ClaimsPrincipal에서 Email 추출
    /// </summary>
    /// <param name="user">ClaimsPrincipal</param>
    /// <returns>Email (추출 실패 시 null)</returns>
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// ClaimsPrincipal에서 Role 추출
    /// </summary>
    /// <param name="user">ClaimsPrincipal</param>
    /// <returns>Role (추출 실패 시 null)</returns>
    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst("role")?.Value ?? user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// ClaimsPrincipal에서 Provider 추출
    /// </summary>
    /// <param name="user">ClaimsPrincipal</param>
    /// <returns>Provider (추출 실패 시 null)</returns>
    public static string? GetProvider(this ClaimsPrincipal user)
    {
        return user.FindFirst("provider")?.Value;
    }
}
