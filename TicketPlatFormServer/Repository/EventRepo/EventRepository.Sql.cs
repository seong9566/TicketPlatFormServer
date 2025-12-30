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
    
    /// <summary>
    /// 이벤트 상세 정보 조회 SQL
    /// </summary>
    private const string SqlGetEventDetailById = @"
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
    
    /// <summary>
    /// 이벤트의 티켓 목록 조회 SQL
    /// </summary>
    private const string SqlGetTicketsByEventId = @"
        SELECT 
            t.id AS TicketId,
            t.title AS TicketTitle,
            t.seat_info AS SeatInfo,
            t.price AS Price,
            t.original_price AS OriginalPrice,
            t.seat_features AS SeatFeatures,
            t.description AS Description,
            t.remaining_quantity AS RemainingQuantity,
            t.created_at AS CreatedAt,
            up.user_id AS UserId,
            up.nickname AS Nickname,
            up.profile_image_url AS ProfileImageUrl,
            up.manner_temperature AS MannerTemperature
        FROM tickets t
        INNER JOIN user_profile up ON t.seller_id = up.user_id
        WHERE t.event_id = @EventId
          AND t.status_id = 1
          AND t.deleted_at IS NULL
          AND t.remaining_quantity > 0
        ORDER BY t.price ASC, t.created_at DESC";
}

