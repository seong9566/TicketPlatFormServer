using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DTO;
using UserEntity = TicketPlatFormServer.DBModel.User;

namespace TicketPlatFormServer.Services.Token;

/// <summary>
/// Token 생성 및 검증 Service 구현체
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenService> _logger;

    public TokenService(JwtSettings jwtSettings, ILogger<TokenService> logger)
    {
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<TokenResponseDto> GenerateTokensAsync(UserEntity user, int refreshTokenExpirationDays)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        return await Task.FromResult(new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60, // 초 단위 변환
            TokenType = "Bearer",
            ExpiresAt = expiresAt
        });
    }

    public string GenerateAccessToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        // Claims 생성
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim("userId", user.Id.ToString()),
            new System.Security.Claims.Claim(ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim("role", user.Role.Code),
            new System.Security.Claims.Claim("provider", user.Provider.Code)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Access Token 생성 완료: UserId={UserId}, Email={Email}", user.Id, user.Email);

        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = _jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(_jwtSettings.ClockSkew)
            }, out SecurityToken validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token 검증 실패: {Message}", ex.Message);
            return false;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = _jwtSettings.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(_jwtSettings.ClockSkew)
            }, out SecurityToken validatedToken);

            var userIdClaim = claimsPrincipal.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token에서 UserId 추출 실패: {Message}", ex.Message);
            return null;
        }
    }
}
