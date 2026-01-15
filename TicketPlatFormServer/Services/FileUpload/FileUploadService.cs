using System.Net;
using Microsoft.AspNetCore.Http;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Services.Storage;

namespace TicketPlatFormServer.Services.FileUpload;

public class FileUploadService(
    IStorageUploader storageUploader,
    ISignedUrlCacheService cacheService,
    SupabaseStorageSettings supabaseSettings,
    ILogger<FileUploadService> logger) : IFileUploadService
{
    /// <summary>
    /// 채팅 이미지 업로드
    /// </summary>
    public async Task<ChatImageUploadResult> UploadChatImageAsync(IFormFile file, long userId, long roomId)
    {
        // 1. 기본 검증 (null, empty, size)
        if (file == null || file.Length == 0)
        {
            throw new AppException("파일이 비어 있습니다.", HttpStatusCode.BadRequest);
        }

        var maxSizeBytes = supabaseSettings.MaxFileSizeMB * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            throw new AppException($"파일 크기는 {supabaseSettings.MaxFileSizeMB}MB를 초과할 수 없습니다.", HttpStatusCode.BadRequest);
        }

        // 2. 확장자 검증
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!supabaseSettings.AllowedExtensions.Contains(fileExtension))
        {
            throw new AppException(
                $"허용되지 않는 파일 형식입니다. 허용된 형식: {string.Join(", ", supabaseSettings.AllowedExtensions)}",
                HttpStatusCode.BadRequest);
        }

        // 3. Magic bytes 검증
        using var stream = file.OpenReadStream();
        var isMagicBytesValid = await MagicBytesValidator.ValidateAsync(stream, fileExtension);
        if (!isMagicBytesValid)
        {
            logger.LogWarning("[FileUploadService] Magic bytes mismatch: Extension={Ext}, ContentType={ContentType}",
                fileExtension, file.ContentType);
            throw new AppException("파일 내용이 확장자와 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        // 4. Object key 생성
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var guid = Guid.NewGuid().ToString("N");
        var fileName = $"{userId}_{timestamp}_{guid}{fileExtension}";
        var objectKey = $"chat/{roomId}/{fileName}";

        try
        {
            // 5. 업로드
            stream.Position = 0;
            await storageUploader.UploadAsync(stream, objectKey, file.ContentType);

            // 6. Signed URL 생성 (업로드 직후용: 긴 만료)
            var signedUrl = await storageUploader.GetSignedUrlAsync(objectKey, supabaseSettings.UploadSignedUrlExpirySec);
            var expiresAt = DateTime.UtcNow.AddSeconds(supabaseSettings.UploadSignedUrlExpirySec);

            // 7. 캐시에 저장
            await cacheService.SetAsync(objectKey, signedUrl, supabaseSettings.UploadSignedUrlExpirySec);

            logger.LogInformation("[FileUploadService.UploadChatImageAsync] Success: ObjectKey={ObjectKey}, Provider={Provider}",
                objectKey, storageUploader.ProviderName);

            return new ChatImageUploadResult(objectKey, signedUrl, expiresAt);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            logger.LogError(ex, "[FileUploadService.UploadChatImageAsync] Upload failed: ObjectKey={ObjectKey}", objectKey);
            throw new AppException("파일 업로드 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// 파일 삭제
    /// </summary>
    public async Task<bool> DeleteFileAsync(string objectKey)
    {
        try
        {
            var result = await storageUploader.DeleteAsync(objectKey);
            logger.LogInformation("[FileUploadService.DeleteFileAsync] Deleted: {ObjectKey}, Result: {Result}", objectKey, result);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FileUploadService.DeleteFileAsync] Delete failed: {ObjectKey}", objectKey);
            return false;
        }
    }

    /// <summary>
    /// 티켓 이미지 배치 업로드 (최대 5개)
    /// </summary>
    public async Task<List<TicketImageUploadResult>> UploadTicketImagesAsync(
        List<IFormFile> files,
        int ticketId,
        int userId)
    {
        // 1. 최대 개수 검증
        const int maxImages = 5;
        if (files.Count > maxImages)
        {
            throw new AppException($"티켓 이미지는 최대 {maxImages}개까지 업로드 가능합니다.", HttpStatusCode.BadRequest);
        }

        var results = new List<TicketImageUploadResult>();

        foreach (var file in files)
        {
            // 2. 기본 검증 (null, empty, size)
            if (file == null || file.Length == 0)
            {
                throw new AppException("파일이 비어 있습니다.", HttpStatusCode.BadRequest);
            }

            var maxSizeBytes = supabaseSettings.MaxFileSizeMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                throw new AppException($"파일 크기는 {supabaseSettings.MaxFileSizeMB}MB를 초과할 수 없습니다.", HttpStatusCode.BadRequest);
            }

            // 3. 확장자 검증
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!supabaseSettings.AllowedExtensions.Contains(fileExtension))
            {
                throw new AppException(
                    $"허용되지 않는 파일 형식입니다. 허용된 형식: {string.Join(", ", supabaseSettings.AllowedExtensions)}",
                    HttpStatusCode.BadRequest);
            }

            // 4. Magic bytes 검증
            using var stream = file.OpenReadStream();
            var isMagicBytesValid = await MagicBytesValidator.ValidateAsync(stream, fileExtension);
            if (!isMagicBytesValid)
            {
                logger.LogWarning("[FileUploadService.UploadTicketImagesAsync] Magic bytes mismatch for ticket: Extension={Ext}, ContentType={ContentType}",
                    fileExtension, file.ContentType);
                throw new AppException("파일 내용이 확장자와 일치하지 않습니다.", HttpStatusCode.BadRequest);
            }

            // 5. Object key 생성
            var guid = Guid.NewGuid().ToString("N");
            var objectKey = $"tickets/{ticketId}/{guid}{fileExtension}";

            try
            {
                // 6. 업로드
                stream.Position = 0;
                await storageUploader.UploadAsync(stream, objectKey, file.ContentType);

                // 7. Signed URL 생성 (업로드 직후용: 긴 만료)
                var signedUrl = await storageUploader.GetSignedUrlAsync(objectKey, supabaseSettings.UploadSignedUrlExpirySec);
                var expiresAt = DateTime.UtcNow.AddSeconds(supabaseSettings.UploadSignedUrlExpirySec);

                // 8. 캐시에 저장
                await cacheService.SetAsync(objectKey, signedUrl, supabaseSettings.UploadSignedUrlExpirySec);

                results.Add(new TicketImageUploadResult(0, objectKey, signedUrl, expiresAt));

                logger.LogInformation("[FileUploadService.UploadTicketImagesAsync] Success: ObjectKey={ObjectKey}, Provider={Provider}",
                    objectKey, storageUploader.ProviderName);
            }
            catch (Exception ex) when (ex is not AppException)
            {
                logger.LogError(ex, "[FileUploadService.UploadTicketImagesAsync] Upload failed: ObjectKey={ObjectKey}", objectKey);
                throw new AppException("파일 업로드 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
            }
        }

        logger.LogInformation("[FileUploadService.UploadTicketImagesAsync] Batch upload complete: TicketId={TicketId}, Count={Count}",
            ticketId, results.Count);

        return results;
    }

    /// <summary>
    /// Signed URL 재발급 (단일)
    /// </summary>
    public async Task<SignedUrlResult> RefreshSignedUrlAsync(string objectKey)
    {
        // 캐시 확인
        var cached = await cacheService.GetAsync(objectKey);
        if (cached != null)
        {
            return new SignedUrlResult(cached, DateTime.UtcNow.AddSeconds(supabaseSettings.ReadSignedUrlExpirySec));
        }

        // 새로 발급
        var signedUrl = await storageUploader.GetSignedUrlAsync(objectKey, supabaseSettings.ReadSignedUrlExpirySec);
        var expiresAt = DateTime.UtcNow.AddSeconds(supabaseSettings.ReadSignedUrlExpirySec);

        await cacheService.SetAsync(objectKey, signedUrl, supabaseSettings.ReadSignedUrlExpirySec);

        return new SignedUrlResult(signedUrl, expiresAt);
    }

    /// <summary>
    /// Signed URL 배치 재발급
    /// </summary>
    public async Task<Dictionary<string, SignedUrlResult>> RefreshSignedUrlsBatchAsync(IEnumerable<string> objectKeys)
    {
        var keysList = objectKeys.ToList();
        var result = new Dictionary<string, SignedUrlResult>();

        // 1. 캐시에서 조회
        var cached = await cacheService.GetBatchAsync(keysList);
        var cacheMissKeys = new List<string>();

        foreach (var (key, url) in cached)
        {
            if (url != null)
            {
                result[key] = new SignedUrlResult(url, DateTime.UtcNow.AddSeconds(supabaseSettings.ReadSignedUrlExpirySec));
            }
            else
            {
                cacheMissKeys.Add(key);
            }
        }

        // 2. 캐시 미스된 키들은 배치 요청
        if (cacheMissKeys.Count > 0)
        {
            var freshUrls = await storageUploader.GetSignedUrlsBatchAsync(cacheMissKeys, supabaseSettings.ReadSignedUrlExpirySec);
            var expiresAt = DateTime.UtcNow.AddSeconds(supabaseSettings.ReadSignedUrlExpirySec);

            foreach (var (key, url) in freshUrls)
            {
                result[key] = new SignedUrlResult(url, expiresAt);
            }

            await cacheService.SetBatchAsync(freshUrls, supabaseSettings.ReadSignedUrlExpirySec);
        }

        logger.LogInformation("[FileUploadService.RefreshSignedUrlsBatchAsync] Total={Total}, CacheHit={Hit}, CacheMiss={Miss}",
            keysList.Count, keysList.Count - cacheMissKeys.Count, cacheMissKeys.Count);

        return result;
    }

    /// <summary>
    /// 프로필 이미지 업로드
    /// </summary>
    public async Task<ProfileImageUploadResult> UploadUserProfileImageAsync(IFormFile file, int userId)
    {
        // 1. 기본 검증 (null, empty, size)
        if (file == null || file.Length == 0)
        {
            throw new AppException("파일이 비어 있습니다.", HttpStatusCode.BadRequest);
        }

        var maxSizeBytes = supabaseSettings.MaxFileSizeMB * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            throw new AppException($"파일 크기는 {supabaseSettings.MaxFileSizeMB}MB를 초과할 수 없습니다.", HttpStatusCode.BadRequest);
        }

        // 2. 확장자 검증
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!supabaseSettings.AllowedExtensions.Contains(fileExtension))
        {
            throw new AppException(
                $"허용되지 않는 파일 형식입니다. 허용된 형식: {string.Join(", ", supabaseSettings.AllowedExtensions)}",
                HttpStatusCode.BadRequest);
        }

        // 3. Magic bytes 검증
        using var stream = file.OpenReadStream();
        var isMagicBytesValid = await MagicBytesValidator.ValidateAsync(stream, fileExtension);
        if (!isMagicBytesValid)
        {
            logger.LogWarning("[FileUploadService.UploadUserProfileImageAsync] Magic bytes mismatch: Extension={Ext}, ContentType={ContentType}",
                fileExtension, file.ContentType);
            throw new AppException("파일 내용이 확장자와 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        // 4. Object key 생성 (profiles/{userId}/{guid}.{ext})
        var guid = Guid.NewGuid().ToString("N");
        var objectKey = $"profiles/{userId}/{guid}{fileExtension}";

        try
        {
            // 5. 업로드
            stream.Position = 0;
            await storageUploader.UploadAsync(stream, objectKey, file.ContentType);

            // 6. Signed URL 생성
            var signedUrl = await storageUploader.GetSignedUrlAsync(objectKey, supabaseSettings.UploadSignedUrlExpirySec);
            var expiresAt = DateTime.UtcNow.AddSeconds(supabaseSettings.UploadSignedUrlExpirySec);

            // 7. 캐시에 저장
            await cacheService.SetAsync(objectKey, signedUrl, supabaseSettings.UploadSignedUrlExpirySec);

            logger.LogInformation("[FileUploadService.UploadUserProfileImageAsync] Success: ObjectKey={ObjectKey}, UserId={UserId}",
                objectKey, userId);

            return new ProfileImageUploadResult(objectKey, signedUrl, expiresAt);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            logger.LogError(ex, "[FileUploadService.UploadUserProfileImageAsync] Upload failed: ObjectKey={ObjectKey}", objectKey);
            throw new AppException("파일 업로드 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }
}
