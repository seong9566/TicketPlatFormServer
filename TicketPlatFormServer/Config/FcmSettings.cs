namespace TicketPlatFormServer.Config;

public class FcmSettings
{
    public string ProjectId { get; set; } = string.Empty;
    public string ServiceAccountJsonPath { get; set; } = string.Empty;
    public string OAuthTokenUrl { get; set; } = "https://oauth2.googleapis.com/token";
    public string ApiBaseUrl { get; set; } = "https://fcm.googleapis.com";
}
