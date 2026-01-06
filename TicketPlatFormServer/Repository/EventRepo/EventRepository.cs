using System.Data;
using System.Text.Json;
using Dapper;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.EventRepo;

/// <summary>
/// 이벤트 관련 Repository 구현체
/// </summary>
public partial class EventRepository : IEventRepository
{
    private readonly TicketContext _db;
    private readonly IDbConnection _dapper;

    public EventRepository(TicketContext db , IDbConnection dapper)
    {
        _db = db;
        _dapper = dapper;
    }

    public async Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId)
    {
        var result = await _dapper.QueryAsync<EventListReadModel>(
            SqlGetEventsByCategoryId,
            new { CategoryId = categoryId }
        );

        return result.ToList();
    }

    public async Task<EventDetailReadModel?> GetEventDetailById(int eventId)
    {
        var result = await _dapper.QueryFirstOrDefaultAsync<EventDetailReadModel>(
            SqlGetEventDetailById,
            new { EventId = eventId }
        );

        return result;
    }
}

