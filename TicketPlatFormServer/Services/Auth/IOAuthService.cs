using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Services.Auth;

public interface IOAuthService
{
    string Provider { get; }
    Task<SocialUserInfoDto> GetUserInfoAsync(string accessToken);
}
