using Microsoft.AspNetCore.Http;

namespace TicketPlatFormServer.Services.FileUpload;

public interface IFileUploadService
{
    /// <summary>
    /// 채팅 이미지 업로드
    /// </summary>
    Task<ChatImageUploadResult> UploadChatImageAsync(IFormFile file, long userId, long roomId);

    /// <summary>
    /// 파일 삭제 (object key 기반)
    /// </summary>
    Task<bool> DeleteFileAsync(string objectKey);

    /// <summary>
    /// Signed URL 재발급 (단일)
    /// </summary>
    Task<SignedUrlResult> RefreshSignedUrlAsync(string objectKey);

    /// <summary>
    /// Signed URL 배치 재발급
    /// </summary>
    Task<Dictionary<string, SignedUrlResult>> RefreshSignedUrlsBatchAsync(IEnumerable<string> objectKeys);
}

public record ChatImageUploadResult(
    string ObjectKey,
    string SignedUrl,
    DateTime ExpiresAt
);

public record SignedUrlResult(
    string SignedUrl,
    DateTime ExpiresAt
);
