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

    /// <summary>
    /// 판매 대시보드 조회 - 기본 쿼리 (GROUP BY 까지, HAVING/ORDER/LIMIT 별도 조합)
    /// </summary>
    internal const string GetSalesDashboardBase = @"
        SELECT
            e.id AS EventId,
            COALESCE(e.title, '공연 정보 없음') AS EventTitle,
            e.poster_image_url AS PosterImageUrl,
            COALESCE(e.venue_name, '장소 미정') AS VenueName,
            MIN(t.event_datetime) AS EarliestEventDatetime,
            SUM(t.quantity) AS TotalCount,
            SUM(
                GREATEST(
                    0,
                    t.quantity - COALESCE((
                        SELECT SUM(ti_active.quantity)
                        FROM transaction_items ti_active
                        INNER JOIN transactions tr_active ON ti_active.transaction_id = tr_active.id
                        INNER JOIN transaction_statuses trs_active ON tr_active.status_id = trs_active.id
                        WHERE ti_active.ticket_id = t.id
                          AND tr_active.deleted_at IS NULL
                          AND tr_active.cancelled_at IS NULL
                          AND trs_active.code IN ('reserved', 'payment_requested', 'pending_payment', 'paid', 'confirmed', 'completed')
                    ), 0)
                )
            ) AS OnSaleCount,
            SUM(
                COALESCE((
                    SELECT SUM(ti.quantity)
                    FROM transaction_items ti
                    INNER JOIN transactions tr ON ti.transaction_id = tr.id
                    INNER JOIN settlements s ON tr.id = s.transaction_id
                    INNER JOIN settlement_statuses ss ON s.status_id = ss.id
                    WHERE ti.ticket_id = t.id
                      AND ss.code = 'completed'
                ), 0)
            ) AS CompletedCount,
            SUM(
                COALESCE((
                    SELECT SUM(ti.quantity)
                    FROM transaction_items ti
                    INNER JOIN transactions tr ON ti.transaction_id = tr.id
                    INNER JOIN transaction_statuses trs ON tr.status_id = trs.id
                    WHERE ti.ticket_id = t.id
                      AND trs.code IN ('paid', 'confirmed', 'completed')
                      AND NOT EXISTS (
                          SELECT 1 FROM settlements s
                          INNER JOIN settlement_statuses ss ON s.status_id = ss.id
                          WHERE s.transaction_id = tr.id AND ss.code = 'completed'
                      )
                ), 0)
            ) AS SettlingCount,
            (
                SELECT CONCAT_WS(' ', esg2.name_ko, a2.area_name, t2.`row`)
                FROM tickets t2
                LEFT JOIN event_seat_grades esg2 ON t2.seat_grade_id = esg2.id
                LEFT JOIN event_seat_areas a2 ON t2.area_id = a2.id
                WHERE t2.seller_id = @SellerId
                  AND t2.event_id = e.id
                  AND t2.deleted_at IS NULL
                  AND t2.status_id != 5
                ORDER BY t2.created_at DESC
                LIMIT 1
            ) AS RepresentativeSeatInfo
        FROM tickets t
        LEFT JOIN events e ON t.event_id = e.id
        WHERE t.seller_id = @SellerId
          AND t.deleted_at IS NULL
          AND t.status_id != 5
        GROUP BY e.id, e.title, e.poster_image_url, e.venue_name";

    /// <summary>
    /// 판매 대시보드 총 공연 개수 (HAVING 필터 포함 가능한 서브쿼리 래퍼)
    /// </summary>
    internal const string GetSalesDashboardCountBase = @"
        SELECT COUNT(*) FROM (
            SELECT e.id,
                SUM(
                    GREATEST(
                        0,
                        t.quantity - COALESCE((
                            SELECT SUM(ti_active.quantity)
                            FROM transaction_items ti_active
                            INNER JOIN transactions tr_active ON ti_active.transaction_id = tr_active.id
                            INNER JOIN transaction_statuses trs_active ON tr_active.status_id = trs_active.id
                            WHERE ti_active.ticket_id = t.id
                              AND tr_active.deleted_at IS NULL
                              AND tr_active.cancelled_at IS NULL
                              AND trs_active.code IN ('reserved', 'payment_requested', 'pending_payment', 'paid', 'confirmed', 'completed')
                        ), 0)
                    )
                ) AS OnSaleCount,
                SUM(
                    COALESCE((
                        SELECT SUM(ti.quantity)
                        FROM transaction_items ti
                        INNER JOIN transactions tr ON ti.transaction_id = tr.id
                        INNER JOIN settlements s ON tr.id = s.transaction_id
                        INNER JOIN settlement_statuses ss ON s.status_id = ss.id
                        WHERE ti.ticket_id = t.id
                          AND ss.code = 'completed'
                    ), 0)
                ) AS CompletedCount,
                SUM(
                    COALESCE((
                        SELECT SUM(ti.quantity)
                        FROM transaction_items ti
                        INNER JOIN transactions tr ON ti.transaction_id = tr.id
                        INNER JOIN transaction_statuses trs ON tr.status_id = trs.id
                        WHERE ti.ticket_id = t.id
                          AND trs.code IN ('paid', 'confirmed', 'completed')
                          AND NOT EXISTS (
                              SELECT 1 FROM settlements s
                              INNER JOIN settlement_statuses ss ON s.status_id = ss.id
                              WHERE s.transaction_id = tr.id AND ss.code = 'completed'
                          )
                    ), 0)
                ) AS SettlingCount
            FROM tickets t
            LEFT JOIN events e ON t.event_id = e.id
            WHERE t.seller_id = @SellerId
              AND t.deleted_at IS NULL
              AND t.status_id != 5
            GROUP BY e.id";

    /// <summary>
    /// 공연별 티켓 목록 조회
    /// </summary>
    internal const string GetEventTickets = @"
        SELECT
            t.id AS TicketId,
            t.event_id AS EventId,
            COALESCE(e.title, '공연 정보 없음') AS EventTitle,
            esg.name_ko AS SeatGradeName,
            a.area_name AS AreaName,
            t.`row` AS `Row`,
            CONCAT_WS(' ', esg.name_ko, a.area_name, t.`row`) AS SeatInfo,
            t.quantity AS Quantity,
            GREATEST(
                0,
                t.quantity - COALESCE((
                    SELECT SUM(ti_active.quantity)
                    FROM transaction_items ti_active
                    INNER JOIN transactions tr_active ON ti_active.transaction_id = tr_active.id
                    INNER JOIN transaction_statuses trs_active ON tr_active.status_id = trs_active.id
                    WHERE ti_active.ticket_id = t.id
                      AND tr_active.deleted_at IS NULL
                      AND tr_active.cancelled_at IS NULL
                      AND trs_active.code IN ('reserved', 'payment_requested', 'pending_payment', 'paid', 'confirmed', 'completed')
                ), 0)
            ) AS RemainingQuantity,
            t.price AS Price,
            esg.original_price AS OriginalPrice,
            ts.code AS StatusCode,
            ts.name_ko AS StatusName,
            trs.code AS TransactionStatusCode,
            trs.name_ko AS TransactionStatusName,
            tr.id AS TransactionId,
            ss.code AS SettlementStatusCode,
            t.created_at AS CreatedAt,
            (SELECT image_url FROM ticket_images WHERE ticket_id = t.id ORDER BY id ASC LIMIT 1) AS ThumbnailPath
        FROM tickets t
        LEFT JOIN events e ON t.event_id = e.id
        LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
        LEFT JOIN event_seat_areas a ON t.area_id = a.id
        INNER JOIN ticket_statuses ts ON t.status_id = ts.id
        LEFT JOIN transaction_items ti ON t.id = ti.ticket_id
        LEFT JOIN transactions tr ON ti.transaction_id = tr.id
        LEFT JOIN transaction_statuses trs ON tr.status_id = trs.id
        LEFT JOIN settlements s ON tr.id = s.transaction_id
        LEFT JOIN settlement_statuses ss ON s.status_id = ss.id
        WHERE t.seller_id = @SellerId
          AND t.event_id = @EventId
          AND t.deleted_at IS NULL
          AND t.status_id != 5
          AND (
              ss.code = 'completed'
              OR trs.code IN ('confirmed', 'paid', 'completed', 'cancelled', 'refunded')
          )
        ORDER BY t.created_at DESC
        LIMIT @Size OFFSET @Offset";

    /// <summary>
    /// 공연별 티켓 총 개수
    /// </summary>
    internal const string GetEventTicketsCount = @"
        SELECT COUNT(*)
        FROM tickets t
        LEFT JOIN transaction_items ti ON t.id = ti.ticket_id
        LEFT JOIN transactions tr ON ti.transaction_id = tr.id
        LEFT JOIN transaction_statuses trs ON tr.status_id = trs.id
        LEFT JOIN settlements s ON tr.id = s.transaction_id
        LEFT JOIN settlement_statuses ss ON s.status_id = ss.id
        WHERE t.seller_id = @SellerId
          AND t.event_id = @EventId
          AND t.deleted_at IS NULL
          AND t.status_id != 5
          AND (
              ss.code = 'completed'
              OR trs.code IN ('confirmed', 'paid', 'completed', 'cancelled', 'refunded')
          )";

}
