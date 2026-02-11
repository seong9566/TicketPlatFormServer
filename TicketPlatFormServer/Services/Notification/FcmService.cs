using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Repository.Notifications;

namespace TicketPlatFormServer.Services.Notification;

public class FcmService(
    IHttpClientFactory httpClientFactory,
    FcmSettings fcmSettings,
    INotificationTokenRepository notificationTokenRepository,
    IWebHostEnvironment environment,
    ILogger<FcmService> logger) : IFcmService
{
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedAccessToken;
    private DateTime _cachedAccessTokenExpiryUtc = DateTime.MinValue;

    public async Task SendToUserAsync(long userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default)
    {
        ValidateSettings();

        var tokens = await notificationTokenRepository.GetByUserIdAsync(userId);
        if (tokens.Count == 0)
        {
            return;
        }

        foreach (var token in tokens)
        {
            await SendToTokenAsync(token.DeviceToken, title, body, data, ct);
        }
    }

    private async Task SendToTokenAsync(string deviceToken, string title, string body, Dictionary<string, string>? data, CancellationToken ct)
    {
        var accessToken = await GetAccessTokenAsync(ct);

        var payload = new
        {
            message = new
            {
                token = deviceToken,
                notification = new { title, body },
                data = data ?? new Dictionary<string, string>()
            }
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{fcmSettings.ApiBaseUrl}/v1/projects/{fcmSettings.ProjectId}/messages:send");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = httpClientFactory.CreateClient("FCM");
        using var response = await client.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        if (content.Contains("UNREGISTERED", StringComparison.OrdinalIgnoreCase))
        {
            await notificationTokenRepository.DeleteByDeviceTokenAsync(deviceToken);
            logger.LogInformation("FCM UNREGISTERED 토큰 정리 완료: {DeviceToken}", deviceToken);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await InvalidateCachedTokenAsync();
        }

        logger.LogWarning("FCM 발송 실패: StatusCode={StatusCode}, Response={Response}", (int)response.StatusCode, content);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_cachedAccessToken) && _cachedAccessTokenExpiryUtc > DateTime.UtcNow.AddMinutes(1))
        {
            return _cachedAccessToken;
        }

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrWhiteSpace(_cachedAccessToken) && _cachedAccessTokenExpiryUtc > DateTime.UtcNow.AddMinutes(1))
            {
                return _cachedAccessToken;
            }

            var serviceAccount = await LoadServiceAccountAsync(ct);
            var assertion = CreateJwtAssertion(serviceAccount);

            using var req = new HttpRequestMessage(HttpMethod.Post, fcmSettings.OAuthTokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["assertion"] = assertion
                })
            };

            var client = httpClientFactory.CreateClient("FCM");
            using var response = await client.SendAsync(req, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new AppException($"FCM OAuth 토큰 발급 실패: {content}", HttpStatusCode.InternalServerError);
            }

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AppException("FCM OAuth 토큰 발급 실패: access_token이 없습니다.", HttpStatusCode.InternalServerError);
            }

            _cachedAccessToken = token;
            _cachedAccessTokenExpiryUtc = DateTime.UtcNow.AddSeconds(Math.Max(60, expiresIn - 60));
            return token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task InvalidateCachedTokenAsync()
    {
        await _tokenLock.WaitAsync();
        try
        {
            _cachedAccessToken = null;
            _cachedAccessTokenExpiryUtc = DateTime.MinValue;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<FcmServiceAccountDto> LoadServiceAccountAsync(CancellationToken ct)
    {
        var path = fcmSettings.ServiceAccountJsonPath;
        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(Path.Combine(environment.ContentRootPath, path));
        }

        if (!File.Exists(path))
        {
            throw new AppException($"FCM 서비스 계정 파일을 찾을 수 없습니다: {path}", HttpStatusCode.InternalServerError);
        }

        var content = await File.ReadAllTextAsync(path, ct);
        var dto = JsonSerializer.Deserialize<FcmServiceAccountDto>(content);
        if (dto == null || string.IsNullOrWhiteSpace(dto.ClientEmail) || string.IsNullOrWhiteSpace(dto.PrivateKey))
        {
            throw new AppException("FCM 서비스 계정 JSON 형식이 올바르지 않습니다.", HttpStatusCode.InternalServerError);
        }

        return dto;
    }

    private static string CreateJwtAssertion(FcmServiceAccountDto account)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = new Dictionary<string, object>
        {
            ["iss"] = account.ClientEmail,
            ["scope"] = "https://www.googleapis.com/auth/firebase.messaging",
            ["aud"] = "https://oauth2.googleapis.com/token",
            ["iat"] = now,
            ["exp"] = now + 3600
        };

        var headerJson = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT"
        });
        var payloadJson = JsonSerializer.Serialize(payload);
        var headerPart = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signingInput = $"{headerPart}.{payloadPart}";

        var privateKeyPem = account.PrivateKey.Replace("\\n", "\n");
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signaturePart = Base64UrlEncode(signature);

        return $"{signingInput}.{signaturePart}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(fcmSettings.ProjectId))
        {
            throw new AppException("FCM:ProjectId 설정이 필요합니다.", HttpStatusCode.InternalServerError);
        }

        if (string.IsNullOrWhiteSpace(fcmSettings.ServiceAccountJsonPath))
        {
            throw new AppException("FCM:ServiceAccountJsonPath 설정이 필요합니다.", HttpStatusCode.InternalServerError);
        }
    }

    private sealed class FcmServiceAccountDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("client_email")]
        public string ClientEmail { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("private_key")]
        public string PrivateKey { get; set; } = string.Empty;
    }
}
