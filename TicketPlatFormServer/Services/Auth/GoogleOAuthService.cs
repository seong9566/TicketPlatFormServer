using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Services.Auth;

public class GoogleOAuthService(IHttpClientFactory httpClientFactory, ILogger<GoogleOAuthService> logger) : IOAuthService
{
    public string Provider => "google";

    public async Task<SocialUserInfoDto> GetUserInfoAsync(string accessToken)
    {
        var token = accessToken.Trim();
        if (string.IsNullOrEmpty(token))
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        var client = httpClientFactory.CreateClient("OAuthProvider");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Google UserInfo API 호출 실패: {StatusCode}", (int)response.StatusCode);
            throw new AppException("소셜 로그인 처리 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (!root.TryGetProperty("sub", out var subElement))
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        var providerId = subElement.GetString();
        if (string.IsNullOrWhiteSpace(providerId))
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        string? email = null;
        if (root.TryGetProperty("email", out var emailElement))
        {
            email = emailElement.GetString();
        }

        var emailVerified = true;
        if (root.TryGetProperty("email_verified", out var emailVerifiedElement) && emailVerifiedElement.ValueKind == JsonValueKind.False)
        {
            emailVerified = false;
        }

        if (!emailVerified)
        {
            email = null;
        }

        var name = "google_user";
        if (root.TryGetProperty("name", out var nameElement) && !string.IsNullOrWhiteSpace(nameElement.GetString()))
        {
            name = nameElement.GetString()!;
        }

        string? profileImageUrl = null;
        if (root.TryGetProperty("picture", out var pictureElement))
        {
            profileImageUrl = pictureElement.GetString();
        }

        return new SocialUserInfoDto
        {
            ProviderId = providerId,
            Email = email,
            Name = name,
            ProfileImageUrl = profileImageUrl
        };
    }
}
