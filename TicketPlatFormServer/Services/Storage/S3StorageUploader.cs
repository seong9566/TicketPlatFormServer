using Amazon.S3;
using Amazon.S3.Model;
using TicketPlatFormServer.Config;

namespace TicketPlatFormServer.Services.Storage;

public class S3StorageUploader : IStorageUploader
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsS3Settings _settings;
    private readonly ILogger<S3StorageUploader> _logger;

    public string ProviderName => "S3";

    public S3StorageUploader(
        IAmazonS3 s3Client,
        AwsS3Settings settings,
        ILogger<S3StorageUploader> logger)
    {
        _s3Client = s3Client;
        _settings = settings;
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest, ct);
        _logger.LogInformation("[S3StorageUploader.UploadAsync] Uploaded: {ObjectKey}", objectKey);

        return objectKey;
    }

    public Task<string> GetSignedUrlAsync(string objectKey, int expirySec, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddSeconds(expirySec)
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task<Dictionary<string, string>> GetSignedUrlsBatchAsync(IEnumerable<string> objectKeys, int expirySec, CancellationToken ct = default)
    {
        var result = new Dictionary<string, string>();
        foreach (var key in objectKeys)
        {
            result[key] = await GetSignedUrlAsync(key, expirySec, ct);
        }
        return result;
    }

    public async Task<bool> DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = objectKey
        };

        var response = await _s3Client.DeleteObjectAsync(deleteRequest, ct);

        _logger.LogInformation("[S3StorageUploader.DeleteAsync] Deleted: {ObjectKey}, Status: {Status}",
            objectKey, response.HttpStatusCode);

        return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent
            || response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
}
