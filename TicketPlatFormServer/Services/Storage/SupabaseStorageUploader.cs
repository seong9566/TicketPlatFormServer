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

    public async Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default)
    {
        var url = $"/storage/v1/object/{_settings.BucketName}/{objectKey}";

        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = content;
        request.Headers.Add("x-upsert", "false");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.UploadTimeoutSec));

        var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("[SupabaseStorageUploader.UploadAsync] Uploaded: {ObjectKey}", objectKey);
        return objectKey;
    }

    public async Task<string> GetSignedUrlAsync(string objectKey, int expirySec, CancellationToken ct = default)
    {
        var url = $"/storage/v1/object/sign/{_settings.BucketName}/{objectKey}";
        var body = new { expiresIn = expirySec };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.SignUrlTimeoutSec));

        var response = await _httpClient.PostAsJsonAsync(url, body, cts.Token);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SignUrlResponse>(ct);
        
        // Supabase API가 반환하는 SignedUrl은 "/object/sign/..." 형태이므로 "/storage/v1" 접두사가 필요함
        // 만약 API 응답에 "/storage/v1"이 포함되어 있다면 중복 방지 로직 필요하겠지만, 현재 확인된 바로는 포함되지 않음
        var signedPath = result!.SignedUrl.StartsWith("/storage/v1") ? result!.SignedUrl : $"/storage/v1{result!.SignedUrl}";
        var signedUrl = $"{_settings.ProjectUrl}{signedPath}";

        return signedUrl;
    }

    public async Task<Dictionary<string, string>> GetSignedUrlsBatchAsync(IEnumerable<string> objectKeys, int expirySec, CancellationToken ct = default)
    {
        var keysList = objectKeys.ToList();
        if (keysList.Count == 0) return new Dictionary<string, string>();

        var url = $"/storage/v1/object/sign/{_settings.BucketName}";
        var body = new { expiresIn = expirySec, paths = keysList };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.SignUrlTimeoutSec * 2));

        var response = await _httpClient.PostAsJsonAsync(url, body, cts.Token);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<BatchSignUrlResponse>>(ct);

        return results!.ToDictionary(
            r => r.Path,
            r => 
            {
                var signedPath = r.SignedUrl.StartsWith("/storage/v1") ? r.SignedUrl : $"/storage/v1{r.SignedUrl}";
                return $"{_settings.ProjectUrl}{signedPath}";
            }
        );
    }

    public async Task<bool> DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        var url = $"/storage/v1/object/{_settings.BucketName}/{objectKey}";

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_settings.DeleteTimeoutSec));

        var response = await _httpClient.DeleteAsync(url, cts.Token);

        _logger.LogInformation("[SupabaseStorageUploader.DeleteAsync] Deleted: {ObjectKey}, Status: {Status}",
            objectKey, response.StatusCode);

        return response.IsSuccessStatusCode;
    }

    private record SignUrlResponse(string SignedUrl);
    private record BatchSignUrlResponse(string Path, string SignedUrl);
}
