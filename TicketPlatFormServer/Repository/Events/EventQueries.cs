namespace TicketPlatFormServer.Repository.Events;

/// <summary>
/// EventRepository SQL 쿼리 모음 (Static Class 패턴)
/// </summary>
internal static class EventQueries
{
    /// <summary>
    /// 카테고리별 공연 목록 조회 SQL
    /// </summary>
    internal const string GetEventsByCategoryId = @"
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

    /// <summary>
    /// 이벤트 상세 정보 조회 SQL (이벤트 정보만)
    /// </summary>
    internal const string GetEventDetailById = @"
        SELECT
            e.id AS EventId,
            e.title AS EventTitle,
            e.poster_image_url AS EventPosterImageUrl,
            e.start_at AS StartAt,
            e.end_at AS EndAt,
            e.venue_name AS VenueName,
            e.venue_address AS VenueAddress,
            a.id AS ArtistId,
            a.name AS ArtistName
        FROM events e
        LEFT JOIN artists a ON e.artist_id = a.id
        WHERE e.id = @EventId
          AND e.is_active = 1";
}
