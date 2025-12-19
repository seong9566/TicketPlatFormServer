using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DTO.Home;

namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// 홈 화면 Repository 구현체
/// </summary>
public class HomeRepository : IHomeRepository
{
    private readonly TicketContext _context;
    private readonly IDbConnection _dapper;

    public HomeRepository(TicketContext context, IDbConnection dapper)
    {
        _context = context;
        _dapper = dapper;
    }

    public async Task<List<PopularTicketDto>> GetPopularTickets(int limit = 10)
    {
        var sql = @"
            SELECT 
                t.id AS TicketId,
                t.title AS TicketTitle,
                t.price AS Price,
                e.poster_image_url AS PosterImageUrl,
                e.title AS EventTitle,
                DATE_FORMAT(t.event_datetime, '%m.%d') AS EventDate
            FROM tickets t
            LEFT JOIN events e ON t.event_id = e.id
            WHERE t.status_id = 1 AND t.deleted_at IS NULL
            ORDER BY t.created_at DESC
            LIMIT @Limit";

    
        var result = await _dapper.QueryAsync<PopularTicketDto>(sql, new { Limit = limit });
        
        return result.ToList();
    }

    public async Task<List<RecommendedEventDto>> GetRecommendedEvents(int? userId = null, int limit = 5)
    {
        string sql;
        
        if (userId.HasValue)
        {
            // 로그인한 사용자: 찜한 이벤트와 같은 카테고리의 다른 이벤트 추천
            sql = @"
                SELECT
                    e.id AS EventId,
                    e.title AS EventTitle,
                    e.poster_image_url AS PosterImageUrl,
                    DATE_FORMAT(e.start_at, '%m.%d') AS EventDate,
                    COUNT(DISTINCT t.id) AS TicketCount
                FROM user_favorites uf
                JOIN events fav_e ON uf.target_id = fav_e.id AND uf.favorite_type_id = 1
                JOIN ticket_category tc ON fav_e.category_id = tc.id
                JOIN events e ON e.category_id = tc.id AND e.id != fav_e.id
                LEFT JOIN tickets t ON t.event_id = e.id
                    AND t.deleted_at IS NULL
                    AND t.status_id = 1
                WHERE uf.user_id = @UserId AND e.is_active = 1
                GROUP BY e.id
                HAVING TicketCount > 0
                ORDER BY TicketCount DESC
                LIMIT @Limit";
        }
        else
        {
            // 비로그인 사용자: 티켓 판매 수 기준 인기 이벤트
            sql = @"
                SELECT 
                    e.id AS EventId,
                    e.title AS EventTitle,
                    e.poster_image_url AS PosterImageUrl,
                    DATE_FORMAT(e.start_at, '%m.%d') AS EventDate,
                    COUNT(DISTINCT t.id) AS TicketCount
                FROM events e
                LEFT JOIN tickets t ON t.event_id = e.id 
                    AND t.deleted_at IS NULL 
                    AND t.status_id = 1
                WHERE e.is_active = 1
                GROUP BY e.id
                HAVING TicketCount > 0
                ORDER BY TicketCount DESC
                LIMIT @Limit";
        }

      
        var result = await _dapper.QueryAsync<RecommendedEventDto>(
            sql, 
            new { UserId = userId, Limit = limit }
        );
        
        return result.ToList();
    }
}

