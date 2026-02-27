using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TicketPlatFormServer.Tests.Helpers;

/// <summary>
/// JWT 토큰 생성 헬퍼 (테스트 전용)
/// appsettings.json의 JwtSettings와 동일한 값 사용
/// </summary>
public static class TestAuthHelper
{
    private const string SecretKey =
        "TicketPlatform-SuperSecret-JWT-Key-Min-256-Bits-For-HS256-Algorithm-2026";
    private const string Issuer = "TicketPlatform";
    private const string Audience = "TicketPlatformClient";

    /// <summary>
    /// 일반 사용자 JWT 토큰 생성
    /// </summary>
    public static string GenerateUserToken(
        int userId,
        string email,
        string role = "user",
        string provider = "local")
    {
        return GenerateToken(userId, email, role, provider);
    }

    /// <summary>
    /// 관리자 JWT 토큰 생성
    /// </summary>
    public static string GenerateAdminToken(int userId, string email)
    {
        return GenerateToken(userId, email, "admin", "local");
    }

    /// <summary>
    /// HttpClient에 Authorization 헤더 추가
    /// </summary>
    public static HttpClient AddAuthHeader(
        HttpClient client,
        int userId,
        string email,
        string role = "user")
    {
        var token = GenerateUserToken(userId, email, role);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateToken(
        int userId,
        string email,
        string role,
        string provider)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),         // ClaimsExtensions.GetUserId()
            new Claim(ClaimTypes.Email, email),             // ClaimsExtensions.GetEmail()
            new Claim("role", role),                        // ClaimsExtensions.GetRole()
            new Claim("provider", provider)                 // ClaimsExtensions.GetProvider()
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60), // 1 hour for tests
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
