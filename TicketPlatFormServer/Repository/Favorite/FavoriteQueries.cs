namespace TicketPlatFormServer.Repository.Favorite;

/// <summary>
/// FavoriteRepository SQL 쿼리 모음 (Static Class 패턴)
/// </summary>
internal static class FavoriteQueries
{
    /// <summary>
    /// 찜 존재 여부 확인 SQL
    /// </summary>
    internal const string CheckFavoriteExists = @"
        SELECT id
        FROM user_favorites
        WHERE user_id = @UserId
          AND favorite_type_id = @FavoriteTypeId
          AND target_id = @TargetId
        LIMIT 1";

    /// <summary>
    /// 찜 추가 SQL
    /// </summary>
    internal const string InsertFavorite = @"
        INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
        VALUES (@UserId, @FavoriteTypeId, @TargetId)";

    /// <summary>
    /// 찜 삭제 SQL
    /// </summary>
    internal const string DeleteFavorite = @"
        DELETE FROM user_favorites
        WHERE user_id = @UserId
          AND favorite_type_id = @FavoriteTypeId
          AND target_id = @TargetId";

    /// <summary>
    /// 티켓 존재 및 판매 가능 여부 확인 SQL
    /// </summary>
    internal const string CheckTicketExists = @"
        SELECT id
        FROM tickets
        WHERE id = @TicketId
          AND status_id = 1
          AND deleted_at IS NULL
          AND remaining_quantity > 0
        LIMIT 1";

    /// <summary>
    /// 사용자가 찜한 티켓 목록 조회 SQL (이벤트 정보 포함)
    /// </summary>
    internal const string GetFavoriteTicketsByUserId = @"
        SELECT
            t.id AS TicketId,
            t.title AS TicketTitle,
            t.seat_info AS SeatInfo,
            CASE
                WHEN t.title LIKE '%VIP%' THEN 'VIP석'
                WHEN t.title LIKE '%R석%' OR t.title LIKE '% R %' THEN 'R석'
                WHEN t.title LIKE '%S석%' OR t.title LIKE '% S %' THEN 'S석'
                WHEN t.title LIKE '%A석%' OR t.title LIKE '% A %' THEN 'A석'
                ELSE NULL
            END AS SeatType,
            t.price AS Price,
            t.original_price AS OriginalPrice,
            t.remaining_quantity AS RemainingQuantity,
            t.created_at AS CreatedAt,
            uf.created_at AS FavoritedAt,
            e.title AS EventTitle,
            DATE_FORMAT(e.start_at, '%Y.%m.%d') AS EventDate,
            e.venue_name AS VenueName,
            e.poster_image_url AS EventPosterImageUrl,
            up.user_id AS SellerId,
            up.nickname AS Nickname,
            up.profile_image_url AS ProfileImageUrl,
            up.manner_temperature AS MannerTemperature,
            up.total_trade_count AS TotalTradeCount,
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
            ) AS ResponseRate,
            CASE
                WHEN uv.identity_verified = 1
                 AND uv.phone_verified = 1
                 AND uv.account_verified = 1
                THEN 1
                ELSE 0
            END AS IsSecurePayment
        FROM user_favorites uf
        INNER JOIN tickets t ON uf.target_id = t.id
        LEFT JOIN events e ON t.event_id = e.id
        INNER JOIN user_profile up ON t.seller_id = up.user_id
        LEFT JOIN user_verification uv ON t.seller_id = uv.user_id
        WHERE uf.user_id = @UserId
          AND uf.favorite_type_id = @FavoriteTypeId
          AND t.status_id = 1
          AND t.deleted_at IS NULL
          AND t.remaining_quantity > 0
        ORDER BY uf.created_at DESC";
}
