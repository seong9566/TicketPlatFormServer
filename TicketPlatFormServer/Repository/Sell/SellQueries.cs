namespace TicketPlatFormServer.Repository.Sell;

/// <summary>
/// SellRepository SQL 쿼리 모음 (Dapper용)
/// </summary>
internal static class SellQueries
{
    /// <summary>
    /// 좌석 등급 옵션 일괄 조회 (통합 테이블 사용)
    /// </summary>
    internal const string GetSeatGradesWithPrices = @"
        SELECT
            esg.id AS GradeId,
            esg.event_id AS EventId,
            esg.seat_grade_id AS SeatGradeId,
            esg.code AS Code,
            esg.name_ko AS NameKo,
            esg.name_en AS NameEn,
            esg.original_price AS OriginalPrice,
            esg.sort_order AS SortOrder
        FROM event_seat_grades esg
        WHERE esg.event_id = @EventId
          AND esg.is_active = 1
        ORDER BY esg.sort_order ASC";

    /// <summary>
    /// 내 판매 티켓 목록 조회 (JOIN + 페이징)
    /// </summary>
    internal const string GetMyTickets = @"
        SELECT
            t.id AS TicketId,
            t.event_id AS EventId,
            e.title AS EventTitle,
            t.seat_grade_id AS SeatGradeId,
            esg.name_ko AS SeatGradeName,
            sa.area_name AS AreaName,
            t.price AS Price,
            t.quantity AS Quantity,
            t.remaining_quantity AS RemainingQuantity,
            ts.id AS StatusId,
            ts.code AS StatusCode,
            ts.name_ko AS StatusName,
            t.created_at AS CreatedAt
        FROM tickets t
        INNER JOIN events e ON t.event_id = e.id
        LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
        LEFT JOIN event_seat_areas sa ON t.area_id = sa.id
        INNER JOIN ticket_statuses ts ON t.status_id = ts.id
        WHERE t.seller_id = @SellerId
          AND t.deleted_at IS NULL
          AND (@Status IS NULL OR ts.code = @Status)
        ORDER BY t.created_at DESC
        LIMIT @Limit OFFSET @Offset";

    /// <summary>
    /// 내 판매 티켓 총 개수
    /// </summary>
    internal const string GetMyTicketsCount = @"
        SELECT COUNT(*)
        FROM tickets t
        INNER JOIN ticket_statuses ts ON t.status_id = ts.id
        WHERE t.seller_id = @SellerId
          AND t.deleted_at IS NULL
          AND (@Status IS NULL OR ts.code = @Status)";
}
