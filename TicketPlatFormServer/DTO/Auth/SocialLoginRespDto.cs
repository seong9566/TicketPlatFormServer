namespace TicketPlatFormServer.DTO;

public class SocialLoginRespDto
{
    public int UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool IsNewUser { get; set; }
}
