namespace TicketPlatFormServer.Config;

public class SupabaseStorageSettings
{
    public string ProjectUrl { get; set; } = null!;
    public string ServiceRoleKey { get; set; } = null!;
    public string BucketName { get; set; } = "chat-images";
    public int MaxFileSizeMB { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    public int UploadSignedUrlExpirySec { get; set; } = 3600;
    public int ReadSignedUrlExpirySec { get; set; } = 1800;
    public int UploadTimeoutSec { get; set; } = 30;
    public int SignUrlTimeoutSec { get; set; } = 5;
    public int DeleteTimeoutSec { get; set; } = 10;
}
