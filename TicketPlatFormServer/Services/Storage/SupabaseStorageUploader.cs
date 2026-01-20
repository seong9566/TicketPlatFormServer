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
        var body = new { expiresIn = expirySec, paths = keysList };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.SignUrlTimeoutSec * 2));

        var response = await _httpClient.PostAsJsonAsync(url, body, cts.Token);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<BatchSignUrlResponse>>(ct);

        if (results == null)
        {
            return new Dictionary<string, string>();
        }

        return results
            .Where(r => !string.IsNullOrEmpty(r.Path) && !string.IsNullOrEmpty(r.SignedUrl))
            .ToDictionary(
                r => r.Path!,
                r =>
                {
                    // r.SignedUrl is checked in Where clause so it is not null here
                    var signedPath = r.SignedUrl!.StartsWith("/storage/v1") ? r.SignedUrl : $"/storage/v1{r.SignedUrl}";
                    return $"{_settings.ProjectUrl}{signedPath}";
                }
            );
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

    private record SignUrlResponse(string? SignedUrl);
    private record BatchSignUrlResponse(string? Path, string? SignedUrl);
}
