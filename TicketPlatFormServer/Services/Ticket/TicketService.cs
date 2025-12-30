using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Repository.Ticket;

namespace TicketPlatFormServer.Services.Ticket;

/// <summary>
/// 티켓 관련 Service 구현체
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;

    public TicketService(ITicketRepository repo)
    {
        _repo = repo;
    }

    public async Task<TicketListRespDto> GetTicketDetailById(int ticketId)
    {
        if (ticketId <= 0)
        {
            throw new AppException(message: "유효하지 않은 티켓 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        var result = await _repo.GetTicketDetailById(ticketId);
        
        if (result == null)
        {
            throw new AppException(message: "티켓을 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        return result;
    }
}
