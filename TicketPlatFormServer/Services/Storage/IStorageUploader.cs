namespace TicketPlatFormServer.Services.Storage;

public interface IStorageUploader
{
    /// <summary>
    /// 파일 업로드 후 object key 반환
    /// </summary>
    Task<string> UploadAsync(Stream stream, string objectKey, string contentType, string? bucketName = null, CancellationToken ct = default);

    /// <summary>
    /// 단일 object key에 대한 signed URL 생성
    /// </summary>
    Task<string> GetSignedUrlAsync(string objectKey, int expirySec, string? bucketName = null, CancellationToken ct = default);

    /// <summary>
    /// 배치 signed URL 생성 (N+1 문제 해결)
    /// </summary>
    Task<Dictionary<string, string>> GetSignedUrlsBatchAsync(IEnumerable<string> objectKeys, int expirySec, string? bucketName = null, CancellationToken ct = default);

    /// <summary>
    /// 파일 삭제
    /// </summary>
    Task<bool> DeleteAsync(string objectKey, string? bucketName = null, CancellationToken ct = default);

    /// <summary>
    /// 버킷 내 파일 목록 조회 (prefix 기반 필터링)
    /// </summary>
    Task<List<StorageObject>> ListObjectsAsync(string prefix, string? bucketName = null, int limit = 1000, int offset = 0, CancellationToken ct = default);

    /// <summary>
    /// Provider 이름 (로깅/메트릭용)
    /// </summary>
    string ProviderName { get; }
}

public record StorageObject(string Name, string Id, DateTime? CreatedAt, long? Size);
