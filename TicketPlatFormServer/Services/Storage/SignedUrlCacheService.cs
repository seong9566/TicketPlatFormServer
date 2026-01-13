using Microsoft.Extensions.Caching.Memory;

namespace TicketPlatFormServer.Services.Storage;

public interface ISignedUrlCacheService
{
    Task<string?> GetAsync(string objectKey);
    Task SetAsync(string objectKey, string signedUrl, int expirySeconds);
    Task<Dictionary<string, string?>> GetBatchAsync(IEnumerable<string> objectKeys);
    Task SetBatchAsync(Dictionary<string, string> urlMap, int expirySeconds);
}

public class SignedUrlCacheService : ISignedUrlCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SignedUrlCacheService> _logger;
    private const string CacheKeyPrefix = "signed_url:";
    private const int ExpiryBufferSeconds = 60;

    public SignedUrlCacheService(IMemoryCache cache, ILogger<SignedUrlCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<string?> GetAsync(string objectKey)
    {
        var cacheKey = $"{CacheKeyPrefix}{objectKey}";
        var cached = _cache.Get<string>(cacheKey);

        if (cached != null)
        {
            _logger.LogDebug("[SignedUrlCacheService] Cache HIT: {ObjectKey}", objectKey);
        }
        else
        {
            _logger.LogDebug("[SignedUrlCacheService] Cache MISS: {ObjectKey}", objectKey);
        }

        return Task.FromResult(cached);
    }

    public Task SetAsync(string objectKey, string signedUrl, int expirySeconds)
    {
        var cacheKey = $"{CacheKeyPrefix}{objectKey}";
        var effectiveExpiry = Math.Max(expirySeconds - ExpiryBufferSeconds, 60);

        _cache.Set(cacheKey, signedUrl, TimeSpan.FromSeconds(effectiveExpiry));

        return Task.CompletedTask;
    }

    public async Task<Dictionary<string, string?>> GetBatchAsync(IEnumerable<string> objectKeys)
    {
        var result = new Dictionary<string, string?>();
        foreach (var key in objectKeys)
        {
            result[key] = await GetAsync(key);
        }
        return result;
    }

    public async Task SetBatchAsync(Dictionary<string, string> urlMap, int expirySeconds)
    {
        foreach (var (key, url) in urlMap)
        {
            await SetAsync(key, url, expirySeconds);
        }
    }
}
