namespace TicketPlatFormServer.Repository.Ticket;

/// <summary>
/// TicketRepository SQL 쿼리 모음 (Static Class 패턴)
/// </summary>
internal static class TicketQueries
{
    /// <summary>
    /// 이벤트의 티켓 목록 조회 SQL (이벤트 상세 화면용 - 간단한 정보만)
    /// </summary>
    internal const string GetTicketsByEventId = @"
        SELECT
            t.id AS TicketId,
            t.title AS TicketTitle,
            t.seat_info AS SeatInfo,
            t.price AS Price,
            t.original_price AS OriginalPrice,
            t.seat_features AS SeatFeatures,
            t.quantity AS Quantity,
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

    /// <summary>
    /// 티켓 상세 정보 조회 SQL (티켓 상세 화면용 - 모든 정보)
    /// </summary>
    internal const string GetTicketDetailById = @"
        SELECT
            t.id AS TicketId,
            t.title AS TicketTitle,
            t.seat_info AS SeatInfo,
            t.price AS Price,
            t.original_price AS OriginalPrice,
            t.seat_features AS SeatFeatures,
            t.description AS Description,
            t.remaining_quantity AS RemainingQuantity,
            t.quantity AS Quantity,
            t.created_at AS CreatedAt,
            up.user_id AS UserId,
            up.nickname AS Nickname,
            up.profile_image_url AS ProfileImageUrl,
            up.manner_temperature AS MannerTemperature,
            up.total_trade_count AS TotalTradeCount,
            CASE
                WHEN uv.identity_verified = 1
                 AND uv.phone_verified = 1
                 AND uv.account_verified = 1
                THEN 1
                ELSE 0
            END AS IsSecurePayment,
            COALESCE(
                (SELECT
                    CASE
                        WHEN COUNT(DISTINCT cm.id) = 0 THEN NULL
                        ELSE ROUND(
                            (COUNT(CASE WHEN cm.sender_id = t.seller_id THEN 1 END) * 100.0 / COUNT(DISTINCT cm.id)),
                            1
                        )
                    END
                 FROM chat_rooms cr
                 INNER JOIN chat_messages cm ON cr.id = cm.room_id
                 WHERE cr.seller_id = t.seller_id
                   AND cr.deleted_at IS NULL
                ), NULL
            ) AS ResponseRate
        FROM tickets t
        INNER JOIN user_profile up ON t.seller_id = up.user_id
        LEFT JOIN user_verification uv ON t.seller_id = uv.user_id
        WHERE t.id = @TicketId
          AND t.status_id = 1
          AND t.deleted_at IS NULL
          AND t.remaining_quantity > 0";

    /// <summary>
    /// 티켓 이미지 조회 SQL
    /// </summary>
    internal const string GetTicketImages = @"
        SELECT image_url
        FROM ticket_images
        WHERE ticket_id = @TicketId
        ORDER BY created_at ASC";
}
