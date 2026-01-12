namespace TicketPlatFormServer.Config;

public class AwsS3Settings
{
    public string BucketName { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string? CloudFrontDomain { get; set; }
    public int MaxFileSizeMB { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif"];
}
