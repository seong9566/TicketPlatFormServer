using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Services.Auth;

public class KakaoOAuthService(IHttpClientFactory httpClientFactory, ILogger<KakaoOAuthService> logger) : IOAuthService
{
    public string Provider => "kakao";

    public async Task<SocialUserInfoDto> GetUserInfoAsync(string accessToken)
    {
        var token = accessToken.Trim();
        if (string.IsNullOrEmpty(token))
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        var client = httpClientFactory.CreateClient("OAuthProvider");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://kapi.kakao.com/v2/user/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Kakao UserInfo API 호출 실패: {StatusCode}", (int)response.StatusCode);
            throw new AppException("소셜 로그인 처리 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (!root.TryGetProperty("id", out var idElement))
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        var providerId = idElement.ToString();
        if (string.IsNullOrWhiteSpace(providerId))
        {
            throw new AppException("유효하지 않은 Access Token입니다.", HttpStatusCode.Unauthorized);
        }

        string? email = null;
        var name = "kakao_user";
        string? profileImageUrl = null;

        if (root.TryGetProperty("kakao_account", out var accountElement) && accountElement.ValueKind == JsonValueKind.Object)
        {
            var isEmailValid = true;
            var isEmailVerified = true;

            if (accountElement.TryGetProperty("is_email_valid", out var emailValidElement) && emailValidElement.ValueKind == JsonValueKind.False)
            {
                isEmailValid = false;
            }

            if (accountElement.TryGetProperty("is_email_verified", out var emailVerifiedElement) && emailVerifiedElement.ValueKind == JsonValueKind.False)
            {
                isEmailVerified = false;
            }

            if (accountElement.TryGetProperty("email", out var emailElement))
            {
                var candidate = emailElement.GetString();
                if (!string.IsNullOrWhiteSpace(candidate) && isEmailValid && isEmailVerified)
                {
                    email = candidate;
                }
            }

            if (accountElement.TryGetProperty("profile", out var profileElement) && profileElement.ValueKind == JsonValueKind.Object)
            {
                if (profileElement.TryGetProperty("nickname", out var nicknameElement) && !string.IsNullOrWhiteSpace(nicknameElement.GetString()))
                {
                    name = nicknameElement.GetString()!;
                }

                if (profileElement.TryGetProperty("profile_image_url", out var profileImageElement))
                {
                    profileImageUrl = profileImageElement.GetString();
                }
            }
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
