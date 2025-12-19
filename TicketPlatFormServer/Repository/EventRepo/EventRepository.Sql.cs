namespace TicketPlatFormServer.Repository.EventRepo;

/// <summary>
/// EventRepository SQL 쿼리 모음
/// Partial 로 분리 시킴.
/// </summary>
public partial class EventRepository
{
    /// <summary>
    /// 카테고리별 공연 목록 조회 SQL
    /// </summary>
    private const string SqlGetEventsByCategoryId = @"
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
}

