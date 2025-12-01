using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Services.User;

public interface IUserService
{
    Task<RegisterUserRespDto> RegisterUser(RegisterUserReqDto dto);
}