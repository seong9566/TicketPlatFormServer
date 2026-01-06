namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// HomeRepository SQL 쿼리 모음 (Static Class 패턴)
/// </summary>
internal static class HomeQueries
{
    /// <summary>
    /// 인기 티켓 목록 조회 SQL
    /// </summary>
    internal const string GetPopularTickets = @"
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

    /// <summary>
    /// 추천 이벤트 목록 조회 SQL (로그인 사용자용 - 찜한 이벤트 기반)
    /// </summary>
    internal const string GetRecommendedEventsForUser = @"
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

    /// <summary>
    /// 추천 이벤트 목록 조회 SQL (비로그인 사용자용 - 인기 이벤트)
    /// </summary>
    internal const string GetRecommendedEventsForGuest = @"
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
