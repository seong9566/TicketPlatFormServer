using System.Data;
using System.Net;
using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Transaction;

namespace TicketPlatFormServer.Repository.Transactions;

public class TransactionHistoryRepository(
    IDbConnection db,
    ILogger<TransactionHistoryRepository> logger) : ITransactionHistoryRepository
{
    public async Task<(List<TransactionHistoryItemDto> Items, int? TotalCount)> GetPurchaseHistoryAsync(
        int userId,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt,
        int limit,
        bool includeTotalCount = false)
    {
        try
        {
            logger.LogDebug(
                "구매 내역 DB 조회 시작 - UserId: {UserId}, StatusFilter: {StatusFilter}, PeriodFilter: {PeriodFilter}, IncludeTotalCount: {IncludeTotalCount}",
                userId, statusFilter, periodFilter, includeTotalCount);

            var (whereClause, parameters) = BuildWhereClause(userId, "t.buyer_id", statusFilter, periodFilter, sortBy, cursorId, cursorCreatedAt);
            var orderByClause = BuildOrderByClause(sortBy);

            // 성능 최적화: 첫 페이지에서만 전체 건수 조회
            int? totalCount = null;
            if (includeTotalCount)
            {
                var countQuery = $@"
                    SELECT COUNT(DISTINCT t.id)
                    FROM transactions t
                    INNER JOIN transaction_statuses ts ON t.status_id = ts.id
                    {whereClause}
                ";

                totalCount = await db.ExecuteScalarAsync<int>(countQuery, parameters);
                logger.LogDebug("구매 내역 전체 건수 조회 - TotalCount: {TotalCount}", totalCount);
            }

            var dataQuery = $@"
                SELECT
                    t.id AS TransactionId,
                    ti.ticket_id AS TicketId,
                    COALESCE(e.title, '티켓 정보 없음') AS TicketTitle,
                    e.poster_image_url AS TicketThumbnailUrl,
                    tick.event_datetime AS EventDateTime,
                    e.venue_name AS VenueName,
                    CONCAT_WS(' ',
                        COALESCE(sl.location_name, ''),
                        COALESCE(a.area_name, ''),
                        COALESCE(sg.name_ko, ''),
                        COALESCE(tick.row, '')
                    ) AS SeatInfo,
                    ti.quantity AS Quantity,
                    ti.unit_price AS UnitPrice,
                    ti.total_price AS TotalAmount,
                    ts.code AS StatusCode,
                    ts.name_ko AS StatusName,
                    cr.id AS RoomId,
                    t.created_at AS CreatedAt,
                    p.paid_at AS PaidAt,
                    t.confirmed_at AS ConfirmedAt,
                    t.cancelled_at AS CancelledAt,
                    t.seller_id AS UserId,
                    COALESCE(up_seller.nickname, '판매자') AS Nickname,
                    up_seller.profile_image_url AS ProfileImageUrl
                FROM transactions t
                INNER JOIN transaction_statuses ts ON t.status_id = ts.id
                INNER JOIN transaction_items ti ON t.id = ti.transaction_id
                INNER JOIN tickets tick ON ti.ticket_id = tick.id
                LEFT JOIN events e ON tick.event_id = e.id
                LEFT JOIN event_seat_locations sl ON tick.seat_location_id = sl.id
                LEFT JOIN event_seat_areas a ON tick.area_id = a.id
                LEFT JOIN event_seat_grades sg ON tick.seat_grade_id = sg.id
                LEFT JOIN user_profile up_seller ON t.seller_id = up_seller.user_id
                LEFT JOIN chat_rooms cr ON t.id = cr.transaction_id
                LEFT JOIN payments p ON t.id = p.transaction_id
                LEFT JOIN payment_statuses ps ON p.status_id = ps.id AND ps.code = 'done'
                {whereClause}
                {orderByClause}
                LIMIT @Limit
            ";

            parameters.Add("@Limit", limit);

            var items = await db.QueryAsync<TransactionHistoryItemDto, TransactionUserDto?, TransactionHistoryItemDto>(
                dataQuery,
                (transaction, seller) =>
                {
                    transaction.Seller = seller;
                    return transaction;
                },
                parameters,
                splitOn: "UserId"
            );

            logger.LogDebug("구매 내역 DB 조회 완료 - UserId: {UserId}, 결과 수: {Count}", userId, items.Count());

            return (items.ToList(), totalCount);
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "구매 내역 DB 조회 중 MySQL 예외 발생 - UserId: {UserId}, ErrorCode: {ErrorCode}, SqlState: {SqlState}",
                userId, ex.ErrorCode, ex.SqlState);

            // MySQL 에러 코드별 처리
            var errorMessage = ex.ErrorCode switch
            {
                MySqlErrorCode.LockWaitTimeout => "데이터베이스 락 대기 시간 초과",
                MySqlErrorCode.LockDeadlock => "데이터베이스 데드락 발생",
                MySqlErrorCode.UnableToConnectToHost => "데이터베이스 연결 실패",
                _ => "데이터베이스 조회 중 오류가 발생했습니다"
            };

            throw new AppException(errorMessage, HttpStatusCode.InternalServerError, ex);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "구매 내역 DB 조회 타임아웃 - UserId: {UserId}", userId);
            throw new AppException("데이터베이스 조회 시간이 초과되었습니다.", HttpStatusCode.RequestTimeout, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "구매 내역 DB 조회 중 예상치 못한 예외 발생 - UserId: {UserId}", userId);
            throw new AppException("데이터베이스 조회 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    public async Task<(List<TransactionHistoryItemDto> Items, int? TotalCount)> GetSalesHistoryAsync(
        int userId,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt,
        int limit,
        bool includeTotalCount = false)
    {
        try
        {
            logger.LogDebug(
                "판매 내역 DB 조회 시작 - UserId: {UserId}, StatusFilter: {StatusFilter}, PeriodFilter: {PeriodFilter}, IncludeTotalCount: {IncludeTotalCount}",
                userId, statusFilter, periodFilter, includeTotalCount);

            var (whereClause, parameters) = BuildWhereClause(userId, "t.seller_id", statusFilter, periodFilter, sortBy, cursorId, cursorCreatedAt);
            var orderByClause = BuildOrderByClause(sortBy);

            // 성능 최적화: 첫 페이지에서만 전체 건수 조회
            int? totalCount = null;
            if (includeTotalCount)
            {
                var countQuery = $@"
                    SELECT COUNT(DISTINCT t.id)
                    FROM transactions t
                    INNER JOIN transaction_statuses ts ON t.status_id = ts.id
                    {whereClause}
                ";

                totalCount = await db.ExecuteScalarAsync<int>(countQuery, parameters);
                logger.LogDebug("판매 내역 전체 건수 조회 - TotalCount: {TotalCount}", totalCount);
            }

            var dataQuery = $@"
                SELECT
                    t.id AS TransactionId,
                    ti.ticket_id AS TicketId,
                    COALESCE(e.title, '티켓 정보 없음') AS TicketTitle,
                    e.poster_image_url AS TicketThumbnailUrl,
                    tick.event_datetime AS EventDateTime,
                    e.venue_name AS VenueName,
                    CONCAT_WS(' ',
                        COALESCE(sl.location_name, ''),
                        COALESCE(a.area_name, ''),
                        COALESCE(sg.name_ko, ''),
                        COALESCE(tick.row, '')
                    ) AS SeatInfo,
                    ti.quantity AS Quantity,
                    ti.unit_price AS UnitPrice,
                    ti.total_price AS TotalAmount,
                    ts.code AS StatusCode,
                    CASE
                        WHEN ts.code = 'confirmed' THEN '판매 완료'
                        ELSE ts.name_ko
                    END AS StatusName,
                    cr.id AS RoomId,
                    t.created_at AS CreatedAt,
                    p.paid_at AS PaidAt,
                    t.confirmed_at AS ConfirmedAt,
                    t.cancelled_at AS CancelledAt,
                    t.buyer_id AS UserId,
                    COALESCE(up_buyer.nickname, '구매자') AS Nickname,
                    up_buyer.profile_image_url AS ProfileImageUrl
                FROM transactions t
                INNER JOIN transaction_statuses ts ON t.status_id = ts.id
                INNER JOIN transaction_items ti ON t.id = ti.transaction_id
                INNER JOIN tickets tick ON ti.ticket_id = tick.id
                LEFT JOIN events e ON tick.event_id = e.id
                LEFT JOIN event_seat_locations sl ON tick.seat_location_id = sl.id
                LEFT JOIN event_seat_areas a ON tick.area_id = a.id
                LEFT JOIN event_seat_grades sg ON tick.seat_grade_id = sg.id
                LEFT JOIN user_profile up_buyer ON t.buyer_id = up_buyer.user_id
                LEFT JOIN chat_rooms cr ON t.id = cr.transaction_id
                LEFT JOIN payments p ON t.id = p.transaction_id
                LEFT JOIN payment_statuses ps ON p.status_id = ps.id AND ps.code = 'done'
                {whereClause}
                {orderByClause}
                LIMIT @Limit
            ";

            parameters.Add("@Limit", limit);

            var items = await db.QueryAsync<TransactionHistoryItemDto, TransactionUserDto?, TransactionHistoryItemDto>(
                dataQuery,
                (transaction, buyer) =>
                {
                    transaction.Buyer = buyer;
                    return transaction;
                },
                parameters,
                splitOn: "UserId"
            );

            logger.LogDebug("판매 내역 DB 조회 완료 - UserId: {UserId}, 결과 수: {Count}", userId, items.Count());

            return (items.ToList(), totalCount);
        }
        catch (MySqlException ex)
        {
            logger.LogError(ex,
                "판매 내역 DB 조회 중 MySQL 예외 발생 - UserId: {UserId}, ErrorCode: {ErrorCode}, SqlState: {SqlState}",
                userId, ex.ErrorCode, ex.SqlState);

            var errorMessage = ex.ErrorCode switch
            {
                MySqlErrorCode.LockWaitTimeout => "데이터베이스 락 대기 시간 초과",
                MySqlErrorCode.LockDeadlock => "데이터베이스 데드락 발생",
                MySqlErrorCode.UnableToConnectToHost => "데이터베이스 연결 실패",
                _ => "데이터베이스 조회 중 오류가 발생했습니다"
            };

            throw new AppException(errorMessage, HttpStatusCode.InternalServerError, ex);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "판매 내역 DB 조회 타임아웃 - UserId: {UserId}", userId);
            throw new AppException("데이터베이스 조회 시간이 초과되었습니다.", HttpStatusCode.RequestTimeout, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "판매 내역 DB 조회 중 예상치 못한 예외 발생 - UserId: {UserId}", userId);
            throw new AppException("데이터베이스 조회 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    private (string WhereClause, DynamicParameters Parameters) BuildWhereClause(
        int userId,
        string userIdColumn,
        string? statusFilter,
        string? periodFilter,
        string sortBy,
        long? cursorId,
        DateTime? cursorCreatedAt)
    {
        var conditions = new List<string>
        {
            $"{userIdColumn} = @UserId",
            "t.deleted_at IS NULL"
        };

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            var statuses = statusFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            conditions.Add("ts.code IN @Statuses");
            parameters.Add("@Statuses", statuses);
        }

        if (!string.IsNullOrWhiteSpace(periodFilter) && periodFilter != "all")
        {
            var startDate = periodFilter switch
            {
                "1w" => DateTime.UtcNow.AddDays(-7),
                "1m" => DateTime.UtcNow.AddMonths(-1),
                "3m" => DateTime.UtcNow.AddMonths(-3),
                "6m" => DateTime.UtcNow.AddMonths(-6),
                _ => DateTime.MinValue
            };

            if (startDate != DateTime.MinValue)
            {
                conditions.Add("t.created_at >= @StartDate");
                parameters.Add("@StartDate", startDate);
            }
        }

        if (cursorId.HasValue && cursorCreatedAt.HasValue)
        {
            if (sortBy == "oldest")
            {
                conditions.Add("(t.created_at > @CursorCreatedAt OR (t.created_at = @CursorCreatedAt AND t.id > @CursorId))");
            }
            else
            {
                conditions.Add("(t.created_at < @CursorCreatedAt OR (t.created_at = @CursorCreatedAt AND t.id < @CursorId))");
            }
            parameters.Add("@CursorId", cursorId.Value);
            parameters.Add("@CursorCreatedAt", cursorCreatedAt.Value);
        }

        var whereClause = "WHERE " + string.Join(" AND ", conditions);
        return (whereClause, parameters);
    }

    private string BuildOrderByClause(string sortBy)
    {
        return sortBy == "oldest"
            ? "ORDER BY t.created_at ASC, t.id ASC"
            : "ORDER BY t.created_at DESC, t.id DESC";
    }
}
