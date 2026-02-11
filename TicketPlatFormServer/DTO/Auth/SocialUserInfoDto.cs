namespace TicketPlatFormServer.DTO;

public class SocialUserInfoDto
{
    public string ProviderId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}
