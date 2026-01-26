using System;

namespace TicketPlatFormServer.Config;

public class SupabaseStorageSettings
{
    public string ProjectUrl { get; set; } = null!;
    public string ServiceRoleKey { get; set; } = null!;

    /// <summary>
    /// 단일 버킷 이름 (하위 호환성을 위해 유지)
    /// </summary>
    [Obsolete("Use BucketNames instead")]
    public string BucketName { get; set; } = "chat-images";

    /// <summary>
    /// 다중 버킷 설정
    /// </summary>
    public BucketNames BucketNames { get; set; } = new();

    public int MaxFileSizeMB { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg", ".heic", ".heif", ".avif"];
    public int UploadSignedUrlExpirySec { get; set; } = 3600;
    public int ReadSignedUrlExpirySec { get; set; } = 1800;
    public int UploadTimeoutSec { get; set; } = 30;
    public int SignUrlTimeoutSec { get; set; } = 5;
    public int DeleteTimeoutSec { get; set; } = 10;

    /// <summary>
    /// 채팅 메시지당 최대 이미지 수
    /// </summary>
    public int MaxChatImagesPerMessage { get; set; } = 5;
}

/// <summary>
/// 이미지 타입별 버킷 이름 설정
/// </summary>
public class BucketNames
{
    public string ProfileImage { get; set; } = "profile-image";
    public string ChatImage { get; set; } = "chat-images";
    public string TicketImage { get; set; } = "ticket-image";
}
