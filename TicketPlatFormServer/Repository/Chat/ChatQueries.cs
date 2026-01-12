namespace TicketPlatFormServer.Repository.Chat;

internal static class ChatQueries
{
    internal const string GetChatRoomsList = @"
        SELECT
            cr.id AS RoomId,
            cr.ticket_id AS TicketId,
            t.title AS TicketTitle,
            cr.last_message_at AS LastMessageAt,
            (SELECT message FROM chat_messages WHERE room_id = cr.id ORDER BY created_at DESC LIMIT 1) AS LastMessage,
            CASE
                WHEN cr.buyer_id = @UserId THEN cr.unread_count_buyer
                ELSE cr.unread_count_seller
            END AS UnreadCount,
            crs.code AS RoomStatusCode,
            crs.name_ko AS RoomStatusName,
            cr.transaction_id AS TransactionId,
            ts.code AS TransactionStatusCode,
            ts.name_ko AS TransactionStatusName,
            CASE
                WHEN cr.buyer_id = @UserId THEN cr.seller_id
                ELSE cr.buyer_id
            END AS OtherUserId,
            CASE
                WHEN cr.buyer_id = @UserId THEN up_seller.nickname
                ELSE up_buyer.nickname
            END AS OtherUserNickname,
            CASE
                WHEN cr.buyer_id = @UserId THEN up_seller.profile_image_url
                ELSE up_buyer.profile_image_url
            END AS OtherUserProfileImage
        FROM chat_rooms cr
        INNER JOIN tickets t ON cr.ticket_id = t.id
        INNER JOIN chat_room_statuses crs ON cr.status_id = crs.id
        LEFT JOIN transactions trans ON cr.transaction_id = trans.id
        LEFT JOIN transaction_statuses ts ON trans.status_id = ts.id
        LEFT JOIN user_profiles up_buyer ON cr.buyer_id = up_buyer.user_id
        LEFT JOIN user_profiles up_seller ON cr.seller_id = up_seller.user_id
        WHERE (cr.buyer_id = @UserId OR cr.seller_id = @UserId)
            AND cr.deleted_at IS NULL
        ORDER BY cr.last_message_at DESC
        LIMIT @PageSize OFFSET @Offset";

    internal const string GetMessagesByRoomId = @"
        SELECT
            cm.id AS MessageId,
            cm.room_id AS RoomId,
            cm.sender_id AS SenderId,
            up.nickname AS SenderNickname,
            up.profile_image_url AS SenderProfileImage,
            cm.message AS Message,
            cm.image_url AS ImageUrl,
            cm.created_at AS CreatedAt
        FROM chat_messages cm
        INNER JOIN user_profiles up ON cm.sender_id = up.user_id
        WHERE cm.room_id = @RoomId
            AND (@LastMessageId IS NULL OR cm.id < @LastMessageId)
        ORDER BY cm.created_at DESC
        LIMIT @Limit";

    internal const string GetExpiredChatRooms = @"
        SELECT cr.id
        FROM chat_rooms cr
        LEFT JOIN transactions t ON cr.transaction_id = t.id
        WHERE cr.deleted_at IS NULL
            AND (cr.closed_at IS NOT NULL OR t.confirmed_at IS NOT NULL)
            AND (
                (cr.closed_at IS NOT NULL AND cr.closed_at < DATE_SUB(NOW(), INTERVAL @RetentionDays DAY))
                OR
                (t.confirmed_at IS NOT NULL AND t.confirmed_at < DATE_SUB(NOW(), INTERVAL @RetentionDays DAY))
            )";
}
