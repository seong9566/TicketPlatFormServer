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

    /// <summary>
    /// 티켓 이미지 배치 업로드 (최대 5개)
    /// </summary>
    Task<List<TicketImageUploadResult>> UploadTicketImagesAsync(
        List<IFormFile> files,
        int ticketId,
        int userId);

    /// <summary>
    /// 프로필 이미지 업로드
    /// </summary>
    /// <param name="file">프로필 이미지 파일</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>업로드된 이미지 정보</returns>
    Task<ProfileImageUploadResult> UploadUserProfileImageAsync(IFormFile file, int userId);
}

public record ChatImageUploadResult(
    string ObjectKey,
    string SignedUrl,
    DateTime ExpiresAt
);

public record TicketImageUploadResult(
    long ImageId,          // DB insert 전에는 0
    string ObjectKey,      // tickets/{ticketId}/{guid}.{ext}
    string SignedUrl,      // 임시 접근 URL
    DateTime ExpiresAt     // URL 만료 시간
);

public record SignedUrlResult(
    string SignedUrl,
    DateTime ExpiresAt
);

public record ProfileImageUploadResult(
    string ObjectKey,      // profiles/{userId}/{guid}.{ext}
    string SignedUrl,      // 임시 접근 URL
    DateTime ExpiresAt     // URL 만료 시간
);
