using System.Data;
using Dapper;
using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Repository.EventRepo;

/// <summary>
/// 이벤트 관련 Repository 구현체
/// </summary>
public class EventRepository : IEventRepository
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
        var sql =  @"
            SELECT 
                e.id AS EventId, 
                e.title AS EventTitle,
                e.poster_image_url AS EventPosterImageUrl,
                e.start_at AS StartAt,
                e.end_at AS EndAt,
                e.venue_name AS VenueName,
                a.id AS ArtistId, 
                a.name AS ArtistName,
                a.profile_image_url AS ArtistProfileImageUrl,
                e.created_at AS EventCreatedAt,
                CASE 
                    WHEN DATEDIFF(NOW(), e.created_at) <= 5 THEN 1 
                    ELSE 0 
                END AS IsNew
            FROM events e
            LEFT JOIN artists a ON e.artist_id = a.id
            WHERE e.category_id = @CategoryId
              AND e.is_active = 1
            ORDER BY e.sort_order ASC, e.start_at ASC";

        var result = await _dapper.QueryAsync<EventListRespDto>(sql, new { CategoryId = categoryId });
        
        return result.ToList();
    }
}

