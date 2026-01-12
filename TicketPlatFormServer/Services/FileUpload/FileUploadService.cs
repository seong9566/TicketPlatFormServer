using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;

namespace TicketPlatFormServer.Services.FileUpload;

public class FileUploadService(
    IAmazonS3 s3Client,
    AwsS3Settings s3Settings,
    ILogger<FileUploadService> logger) : IFileUploadService
{
    /// <summary>
    /// 채팅 이미지 업로드
    /// </summary>
    public async Task<string> UploadChatImageAsync(IFormFile file, long userId, long roomId)
    {
        // 파일 검증
        if (file == null || file.Length == 0)
        {
            throw new AppException("파일이 비어 있습니다.", HttpStatusCode.BadRequest);
        }

        // 파일 크기 검증
        var maxSizeBytes = s3Settings.MaxFileSizeMB * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            throw new AppException($"파일 크기는 {s3Settings.MaxFileSizeMB}MB를 초과할 수 없습니다.", HttpStatusCode.BadRequest);
        }

        // 파일 확장자 검증
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!s3Settings.AllowedExtensions.Contains(fileExtension))
        {
            throw new AppException(
                $"허용되지 않는 파일 형식입니다. 허용된 형식: {string.Join(", ", s3Settings.AllowedExtensions)}",
                HttpStatusCode.BadRequest);
        }

        try
        {
            // 고유 파일명 생성: chat/{roomId}/{userId}_{timestamp}_{guid}.{ext}
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var guid = Guid.NewGuid().ToString("N");
            var fileName = $"{userId}_{timestamp}_{guid}{fileExtension}";
            var s3Key = $"chat/{roomId}/{fileName}";

            // S3 업로드
            using var stream = file.OpenReadStream();
            var putRequest = new PutObjectRequest
            {
                BucketName = s3Settings.BucketName,
                Key = s3Key,
                InputStream = stream,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead // 공개 읽기 권한
            };

            var response = await s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                logger.LogError("[FileUploadService.UploadChatImageAsync] S3 업로드 실패: {StatusCode}", response.HttpStatusCode);
                throw new AppException("파일 업로드에 실패했습니다.", HttpStatusCode.InternalServerError);
            }

            // URL 생성
            var fileUrl = !string.IsNullOrEmpty(s3Settings.CloudFrontDomain)
                ? $"https://{s3Settings.CloudFrontDomain}/{s3Key}"
                : $"https://{s3Settings.BucketName}.s3.{s3Settings.Region}.amazonaws.com/{s3Key}";

            logger.LogInformation("[FileUploadService.UploadChatImageAsync] 파일 업로드 성공: {FileUrl}", fileUrl);

            return fileUrl;
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "[FileUploadService.UploadChatImageAsync] AWS S3 에러");
            throw new AppException("파일 업로드 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            logger.LogError(ex, "[FileUploadService.UploadChatImageAsync] 파일 업로드 에러");
            throw new AppException("파일 업로드 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// 파일 삭제
    /// </summary>
    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // URL에서 S3 키 추출
            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = s3Settings.BucketName,
                Key = key
            };

            var response = await s3Client.DeleteObjectAsync(deleteRequest);

            logger.LogInformation("[FileUploadService.DeleteFileAsync] 파일 삭제 성공: {FileUrl}", fileUrl);

            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent ||
                   response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError(ex, "[FileUploadService.DeleteFileAsync] AWS S3 에러: {FileUrl}", fileUrl);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FileUploadService.DeleteFileAsync] 파일 삭제 에러: {FileUrl}", fileUrl);
            return false;
        }
    }
}
