namespace TicketPlatFormServer.Repository.Settlements;

internal static class SettlementQueries
{
    internal const string GetBySellerIdList = @"
        SELECT s.id              AS Id,
               s.transaction_id  AS TransactionId,
               s.amount          AS Amount,
               s.fee             AS Fee,
               s.net_amount      AS NetAmount,
               ss.code           AS StatusCode,
               ss.name_ko        AS StatusName,
               s.scheduled_at    AS ScheduledAt,
               s.processed_at    AS ProcessedAt,
               s.failure_reason  AS FailureReason,
               s.retry_count     AS RetryCount,
               s.created_at      AS CreatedAt,
                e.title           AS EventTitle,
                tk.row            AS SeatInfo
        FROM settlements s
        INNER JOIN settlement_statuses ss   ON s.status_id       = ss.id
        INNER JOIN transactions tr          ON s.transaction_id  = tr.id
        INNER JOIN transaction_items t_item ON tr.id             = t_item.transaction_id
        INNER JOIN tickets tk               ON t_item.ticket_id  = tk.id
        INNER JOIN events e                 ON tk.event_id       = e.id
        WHERE s.seller_id = @SellerId
          AND (@StatusFilter IS NULL OR ss.code = @StatusFilter)
        ORDER BY s.created_at DESC
        LIMIT @PageSize OFFSET @Offset";

    internal const string CountBySellerId = @"
        SELECT COUNT(*)
        FROM settlements s
        INNER JOIN settlement_statuses ss   ON s.status_id       = ss.id
        INNER JOIN transactions tr          ON s.transaction_id  = tr.id
        INNER JOIN transaction_items t_item ON tr.id             = t_item.transaction_id
        INNER JOIN tickets tk               ON t_item.ticket_id  = tk.id
        INNER JOIN events e                 ON tk.event_id       = e.id
        WHERE s.seller_id = @SellerId
          AND (@StatusFilter IS NULL OR ss.code = @StatusFilter)";

    internal const string GetTotalCompletedNetAmount = @"
        SELECT COALESCE(SUM(s.net_amount), 0)
        FROM settlements s
        INNER JOIN settlement_statuses ss ON s.status_id = ss.id
        WHERE s.seller_id = @SellerId
          AND ss.code = 'completed'";

    internal const string GetDetailByIdAndSellerId = @"
        SELECT s.id              AS Id,
               s.transaction_id  AS TransactionId,
               s.amount          AS Amount,
               s.fee             AS Fee,
               s.net_amount      AS NetAmount,
               ss.code           AS StatusCode,
               ss.name_ko        AS StatusName,
               s.scheduled_at    AS ScheduledAt,
               s.processed_at    AS ProcessedAt,
               s.failure_reason  AS FailureReason,
               s.retry_count     AS RetryCount,
               s.created_at      AS CreatedAt,
                e.title           AS EventTitle,
                tk.row            AS SeatInfo,
                ba.bank_name      AS BankName,
               ba.account_number AS AccountNumber,
               ba.account_holder AS AccountHolder,
                up.nickname       AS BuyerNickname
        FROM settlements s
        INNER JOIN settlement_statuses ss   ON s.status_id       = ss.id
        INNER JOIN transactions tr          ON s.transaction_id  = tr.id
        INNER JOIN transaction_items t_item ON tr.id             = t_item.transaction_id
        INNER JOIN tickets tk               ON t_item.ticket_id  = tk.id
        INNER JOIN events e                 ON tk.event_id       = e.id
        LEFT  JOIN bank_account ba          ON s.bank_account_id = ba.id
        LEFT  JOIN user_profile up          ON tr.buyer_id       = up.user_id
        WHERE s.id = @SettlementId
          AND s.seller_id = @SellerId";

    internal const string HasBalanceTransaction = @"
        SELECT COUNT(*) > 0
        FROM balance_transactions
        WHERE reference_type = 'SETTLEMENT'
          AND reference_id = @SettlementId";
}
