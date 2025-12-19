using System.Data;
using Dapper;
using TicketPlatFormServer.DTO;

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

    public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)
    { 
        var result = await _dapper.QueryAsync<EventListRespDto>(
            SqlGetEventsByCategoryId, 
            new { CategoryId = categoryId }
        );
        
        return result.ToList();
    }
}

