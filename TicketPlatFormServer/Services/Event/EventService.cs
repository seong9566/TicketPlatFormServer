using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Repository.EventRepo;

namespace TicketPlatFormServer.Services.Event;

/// <summary>
/// 이벤트 관련 Service 구현체
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _repo;

    public EventService(IEventRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new AppException(message: "유효하지 않은 카테고리 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        var result = await _repo.GetEventsByCategoryId(categoryId);
        return result;
    }
}

