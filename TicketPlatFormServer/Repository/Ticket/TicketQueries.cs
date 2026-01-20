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
            -- 좌석 등급 정보 (확장)
            t.seat_grade_id AS SeatGradeId,
            esg.code AS SeatGradeCode,
            esg.name_ko AS SeatGradeName,
            esg.name_en AS SeatGradeNameEn,
            esg.sort_order AS SeatGradeSortOrder,
            -- 구역 정보 (확장)
            t.area_id AS AreaId,
            sa.area_name AS Area,
            sa.sort_order AS AreaSortOrder,
            -- 위치 정보 (NEW)
            t.seat_location_id AS LocationId,
            esl.location_name AS LocationName,
            esl.sort_order AS LocationSortOrder,
            -- 기존 필드
            t.`row` AS `Row`,
            t.price AS Price,
            COALESCE(esg.original_price, t.price) AS OriginalPrice,
            t.quantity AS Quantity,
            t.remaining_quantity AS RemainingQuantity,
            t.is_consecutive AS IsConsecutive,
            t.trade_method_id AS TradeMethodId,
            tm.name_ko AS TradeMethodName,
            t.has_ticket AS HasTicket,
            t.description AS Description,
            t.created_at AS CreatedAt,
            t.feature_ids AS FeatureIds,
            -- 판매자 정보
            up.user_id AS UserId,
            up.nickname AS Nickname,
            up.profile_image_url AS ProfileImageUrl,
            up.manner_temperature AS MannerTemperature
        FROM tickets t
        INNER JOIN user_profile up ON t.seller_id = up.user_id
        LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
        LEFT JOIN event_seat_areas sa ON t.area_id = sa.id
        LEFT JOIN event_seat_locations esl ON t.seat_location_id = esl.id
        LEFT JOIN trade_methods tm ON t.trade_method_id = tm.id
        WHERE t.event_id = @EventId
          AND t.status_id = 1
          AND t.deleted_at IS NULL
          AND t.remaining_quantity > 0
        ORDER BY
            COALESCE(esl.sort_order, 999) ASC,
            COALESCE(esg.sort_order, 999) ASC,
            COALESCE(sa.sort_order, 999) ASC,
            t.price ASC,
            t.created_at DESC";

    /// <summary>
    /// 티켓 상세 정보 조회 SQL (티켓 상세 화면용 - 모든 정보)
    /// </summary>
    internal const string GetTicketDetailById = @"
        SELECT
            t.id AS TicketId,
            -- 좌석 등급 정보 (확장)
            t.seat_grade_id AS SeatGradeId,
            esg.code AS SeatGradeCode,
            esg.name_ko AS SeatGradeName,
            esg.name_en AS SeatGradeNameEn,
            esg.sort_order AS SeatGradeSortOrder,
            -- 구역 정보 (확장)
            t.area_id AS AreaId,
            sa.area_name AS Area,
            sa.sort_order AS AreaSortOrder,
            -- 위치 정보 (NEW)
            t.seat_location_id AS LocationId,
            esl.location_name AS LocationName,
            esl.sort_order AS LocationSortOrder,
            -- 기존 필드
            t.`row` AS `Row`,
            t.price AS Price,
            COALESCE(esg.original_price, t.price) AS OriginalPrice,
            t.quantity AS Quantity,
            t.remaining_quantity AS RemainingQuantity,
            t.is_consecutive AS IsConsecutive,
            t.trade_method_id AS TradeMethodId,
            tm.name_ko AS TradeMethodName,
            t.has_ticket AS HasTicket,
            t.description AS Description,
            t.created_at AS CreatedAt,
            t.feature_ids AS FeatureIds,
            -- 판매자 정보
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
        LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
        LEFT JOIN event_seat_areas sa ON t.area_id = sa.id
        LEFT JOIN event_seat_locations esl ON t.seat_location_id = esl.id
        LEFT JOIN trade_methods tm ON t.trade_method_id = tm.id
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

    /// <summary>
    /// 특정 티켓들의 특이사항 목록 조회 (ID 리스트 기반)
    /// </summary>
    internal const string GetTicketFeaturesByIds = @"
        SELECT
            f.id AS FeatureId,
            f.name_ko AS NameKo,
            f.code AS Code
        FROM ticket_features f
        WHERE f.id IN @Ids";

    /// <summary>
    /// 티켓 특징 조회 SQL (Many-to-Many 관계)
    /// </summary>
    internal const string GetTicketFeatures = @"
        SELECT
            tf.id AS FeatureId,
            tf.code AS Code,
            tf.name_ko AS NameKo,
            tf.name_en AS NameEn,
            tf.icon AS Icon
        FROM ticket_ticket_features ttf
        INNER JOIN ticket_features tf ON ttf.feature_id = tf.id
        WHERE ttf.ticket_id = @TicketId
        ORDER BY tf.sort_order ASC";
}
