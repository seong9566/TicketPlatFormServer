using System.Net.Http.Headers;
using System.Net.Http.Json;
using TicketPlatFormServer.Config;

namespace TicketPlatFormServer.Services.Storage;

public class SupabaseStorageUploader : IStorageUploader
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseStorageSettings _settings;
    private readonly ILogger<SupabaseStorageUploader> _logger;

    public string ProviderName => "Supabase";

    public SupabaseStorageUploader(
        HttpClient httpClient,
        SupabaseStorageSettings settings,
        ILogger<SupabaseStorageUploader> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_settings.ProjectUrl);
        _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ServiceRoleKey);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ServiceRoleKey);
    }

    public async Task<string> UploadAsync(Stream stream, string objectKey, string contentType, string? bucketName = null, CancellationToken ct = default)
    {
        var effectiveBucket = GetEffectiveBucketName(bucketName);
        var url = $"/storage/v1/object/{effectiveBucket}/{objectKey}";

        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = content;
        request.Headers.Add("x-upsert", "false");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.UploadTimeoutSec));

        var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("[SupabaseStorageUploader.UploadAsync] Uploaded: {ObjectKey} to bucket: {Bucket}", objectKey, effectiveBucket);
        return objectKey;
    }

    public async Task<string> GetSignedUrlAsync(string objectKey, int expirySec, string? bucketName = null, CancellationToken ct = default)
    {
        var effectiveBucket = GetEffectiveBucketName(bucketName);
        var url = $"/storage/v1/object/sign/{effectiveBucket}/{objectKey}";
        var body = new { expiresIn = expirySec };

        _logger.LogInformation("[SupabaseStorageUploader.GetSignedUrlAsync] Requesting signed URL: Bucket={Bucket}, ObjectKey={ObjectKey}, ExpiresIn={ExpiresIn}",
            effectiveBucket, objectKey, expirySec);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.SignUrlTimeoutSec));

        var response = await _httpClient.PostAsJsonAsync(url, body, cts.Token);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("[SupabaseStorageUploader.GetSignedUrlAsync] Failed: StatusCode={StatusCode}, Bucket={Bucket}, ObjectKey={ObjectKey}, Error={Error}",
                response.StatusCode, effectiveBucket, objectKey, errorContent);
            
            throw new HttpRequestException($"Supabase signed URL 요청 실패 (Bucket: {effectiveBucket}, Key: {objectKey}): {response.StatusCode} - {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<SignUrlResponse>(ct);

        if (result?.SignedUrl == null)
        {
            throw new HttpRequestException($"Supabase signed URL 응답이 올바르지 않습니다. (Bucket: {effectiveBucket}, Key: {objectKey})");
        }

        var signedPath = result.SignedUrl.StartsWith("/storage/v1") ? result.SignedUrl : $"/storage/v1{result.SignedUrl}";
        var signedUrl = $"{_settings.ProjectUrl}{signedPath}";

        return signedUrl;
    }

    public async Task<Dictionary<string, string>> GetSignedUrlsBatchAsync(IEnumerable<string> objectKeys, int expirySec, string? bucketName = null, CancellationToken ct = default)
    {
        var keysList = objectKeys.ToList();
        if (keysList.Count == 0) return new Dictionary<string, string>();

        var effectiveBucket = GetEffectiveBucketName(bucketName);
        var url = $"/storage/v1/object/sign/{effectiveBucket}";

        // Supabase API에 전달할 때 object key를 그대로 사용 (prefix 유지)
        // 응답의 path와 매칭을 위한 identity mapping
        var body = new { expiresIn = expirySec, paths = keysList };

        _logger.LogInformation("[SupabaseStorageUploader.GetSignedUrlsBatchAsync] Requesting batch signed URLs: Bucket={Bucket}, Count={Count}, Keys=[{Keys}]",
            effectiveBucket, keysList.Count, string.Join(", ", keysList.Take(3)));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.SignUrlTimeoutSec * 2));

        var response = await _httpClient.PostAsJsonAsync(url, body, cts.Token);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<BatchSignUrlResponse>>(ct);

        if (results == null)
        {
            _logger.LogWarning("[SupabaseStorageUploader.GetSignedUrlsBatchAsync] Null response from Supabase");
            return new Dictionary<string, string>();
        }

        var signedUrls = new Dictionary<string, string>();

        foreach (var result in results)
        {
            _logger.LogDebug("[SupabaseStorageUploader.GetSignedUrlsBatchAsync] Response item: Path={Path}, SignedUrl={SignedUrlExists}",
                result.Path, !string.IsNullOrEmpty(result.SignedUrl));

            if (string.IsNullOrEmpty(result.Path) || string.IsNullOrEmpty(result.SignedUrl))
            {
                _logger.LogWarning("[SupabaseStorageUploader.GetSignedUrlsBatchAsync] Failed to sign: Path={Path}, Error={Error}",
                    result.Path, result.Error ?? "unknown");
                continue;
            }

            // 응답의 Path가 요청한 key와 일치하는지 확인
            if (keysList.Contains(result.Path))
            {
                var signedPath = result.SignedUrl.StartsWith("/storage/v1")
                    ? result.SignedUrl
                    : $"/storage/v1{result.SignedUrl}";

                signedUrls[result.Path] = $"{_settings.ProjectUrl}{signedPath}";
            }
            else
            {
                // 경로 매칭 실패 - 요청한 키 목록과 비교
                _logger.LogWarning("[SupabaseStorageUploader.GetSignedUrlsBatchAsync] Unexpected path in response: {Path}. Requested keys: [{Keys}]",
                    result.Path, string.Join(", ", keysList.Take(5)));
            }
        }

        _logger.LogInformation("[SupabaseStorageUploader.GetSignedUrlsBatchAsync] Successfully signed {Count}/{Total} URLs",
            signedUrls.Count, keysList.Count);

        return signedUrls;
    }

    public async Task<bool> DeleteAsync(string objectKey, string? bucketName = null, CancellationToken ct = default)
    {
        var effectiveBucket = GetEffectiveBucketName(bucketName);
        var url = $"/storage/v1/object/{effectiveBucket}/{objectKey}";

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.DeleteTimeoutSec));

        var response = await _httpClient.DeleteAsync(url, cts.Token);

        _logger.LogInformation("[SupabaseStorageUploader.DeleteAsync] Deleted: {ObjectKey} from bucket: {Bucket}, Status: {Status}",
            objectKey, effectiveBucket, response.StatusCode);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// 버킷명이 지정되지 않은 경우 기본 버킷 사용 (하위 호환성)
    /// </summary>
    private string GetEffectiveBucketName(string? bucketName)
    {
#pragma warning disable CS0618 // BucketName is obsolete
        return bucketName ?? _settings.BucketName;
#pragma warning restore CS0618
    }

    /// <summary>
    /// Object key에서 버킷 prefix 제거
    /// 예: "tickets/34/abc.jpg" -> "34/abc.jpg" (when bucket = "ticket-images")
    /// </summary>
    private string StripBucketPrefix(string objectKey, string bucketName)
    {
        // 각 버킷의 prefix 패턴
        var bucketPrefixes = new Dictionary<string, string>
        {
            { _settings.BucketNames.ProfileImage, "profiles/" },
            { _settings.BucketNames.ChatImage, "chat/" },
            { _settings.BucketNames.TicketImage, "tickets/" }
        };

        // 현재 버킷에 해당하는 prefix 찾기
        if (bucketPrefixes.TryGetValue(bucketName, out var prefix))
        {
            if (objectKey.StartsWith(prefix))
            {
                return objectKey.Substring(prefix.Length);
            }
        }

        // prefix가 없거나 매칭되지 않으면 그대로 반환
        return objectKey;
    }

    private record SignUrlResponse(string? SignedUrl);
    private record BatchSignUrlResponse(string? Path, string? SignedUrl, string? Error);
}
