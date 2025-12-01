using TicketPlatFormServer.Enum;

namespace TicketPlatFormServer.DTO;

public class RegisterUserRespDto
{
    public string Email { get; set; }
    public string Phone { get; set; }
    public UserRoleEnum Role { get; set; }
    public UserRegisterProviderEnum Provider { get; set; }
}