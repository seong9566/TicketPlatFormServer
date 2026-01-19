using System.Data;
using System.Text.Json;
using Dapper;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Events;

/// <summary>
/// 이벤트 관련 Repository 구현체 (Primary Constructor + Static Class 패턴)
/// </summary>
public class EventRepository(IDbConnection dapper) : IEventRepository
{
    public async Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId)
    {
        var result = await dapper.QueryAsync<EventListReadModel>(
            EventQueries.GetEventsByCategoryId,
            new { CategoryId = categoryId }
        );

        return result.ToList();
    }

    public async Task<EventDetailReadModel?> GetEventDetailById(int eventId)
    {
        var result = await dapper.QueryFirstOrDefaultAsync<EventDetailReadModel>(
            EventQueries.GetEventDetailById,
            new { EventId = eventId }
        );

        return result;
    }
}
