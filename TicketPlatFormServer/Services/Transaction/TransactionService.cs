using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Transaction;
using TicketPlatFormServer.Repository.Transactions;

namespace TicketPlatFormServer.Services.Transaction;

public class TransactionService(
    ITransactionHistoryRepository repository,
    ILogger<TransactionService> logger) : ITransactionService
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 50;

    // 유효한 status 값 목록
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "reserved", "pending_payment", "paid", "confirmed", "completed", "cancelled", "refunded"
    };

    // 유효한 period 값 목록
    private static readonly HashSet<string> ValidPeriods = new(StringComparer.OrdinalIgnoreCase)
    {
        "1w", "1m", "3m", "6m", "all"
    };

    public async Task<TransactionHistoryRespDto> GetPurchaseHistoryAsync(
        int userId,
        string? status,
        string? period,
        string? sortBy,
        string? cursor,
        int? limit)
    {
        logger.LogInformation(
            "구매 내역 조회 시작 - UserId: {UserId}, Status: {Status}, Period: {Period}, SortBy: {SortBy}",
            userId, status, period, sortBy);

        try
        {
            // 파라미터 검증
            ValidateStatusParameter(status);
            ValidatePeriodParameter(period);

            var (cursorId, cursorCreatedAt) = ParseCursor(cursor);
            var actualLimit = Math.Min(limit ?? DefaultLimit, MaxLimit);
            var actualSortBy = sortBy ?? "latest";

            if (actualSortBy != "latest" && actualSortBy != "oldest")
            {
                throw new AppException("sortBy는 'latest' 또는 'oldest'만 가능합니다.", HttpStatusCode.BadRequest);
            }

            // 성능 최적화: 첫 페이지(cursor가 없는 경우)에서만 전체 건수 조회
            var isFirstPage = string.IsNullOrWhiteSpace(cursor);

            var (items, totalCount) = await repository.GetPurchaseHistoryAsync(
                userId,
                status,
                period,
                actualSortBy,
                cursorId,
                cursorCreatedAt,
                actualLimit + 1,
                includeTotalCount: isFirstPage
            );

            var hasMore = items.Count > actualLimit;
            if (hasMore)
            {
                items = items.Take(actualLimit).ToList();
            }

            string? nextCursor = null;
            if (hasMore && items.Count > 0)
            {
                var lastItem = items[^1];
                nextCursor = CreateCursor(lastItem.TransactionId, lastItem.CreatedAt);
            }

            logger.LogInformation(
                "구매 내역 조회 성공 - UserId: {UserId}, 조회된 항목 수: {Count}, 전체 수: {TotalCount}",
                userId, items.Count, totalCount?.ToString() ?? "N/A");

            return new TransactionHistoryRespDto
            {
                Items = items,
                NextCursor = nextCursor,
                HasMore = hasMore,
                TotalCount = totalCount
            };
        }
        catch (AppException)
        {
            // AppException은 그대로 전파
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "구매 내역 조회 중 예외 발생 - UserId: {UserId}", userId);
            throw new AppException("구매 내역 조회 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    public async Task<TransactionHistoryRespDto> GetSalesHistoryAsync(
        int userId,
        string? status,
        string? period,
        string? sortBy,
        string? cursor,
        int? limit)
    {
        logger.LogInformation(
            "판매 내역 조회 시작 - UserId: {UserId}, Status: {Status}, Period: {Period}, SortBy: {SortBy}",
            userId, status, period, sortBy);

        try
        {
            // 파라미터 검증
            ValidateStatusParameter(status);
            ValidatePeriodParameter(period);

            var (cursorId, cursorCreatedAt) = ParseCursor(cursor);
            var actualLimit = Math.Min(limit ?? DefaultLimit, MaxLimit);
            var actualSortBy = sortBy ?? "latest";

            if (actualSortBy != "latest" && actualSortBy != "oldest")
            {
                throw new AppException("sortBy는 'latest' 또는 'oldest'만 가능합니다.", HttpStatusCode.BadRequest);
            }

            // 성능 최적화: 첫 페이지(cursor가 없는 경우)에서만 전체 건수 조회
            var isFirstPage = string.IsNullOrWhiteSpace(cursor);

            var (items, totalCount) = await repository.GetSalesHistoryAsync(
                userId,
                status,
                period,
                actualSortBy,
                cursorId,
                cursorCreatedAt,
                actualLimit + 1,
                includeTotalCount: isFirstPage
            );

            var hasMore = items.Count > actualLimit;
            if (hasMore)
            {
                items = items.Take(actualLimit).ToList();
            }

            string? nextCursor = null;
            if (hasMore && items.Count > 0)
            {
                var lastItem = items[^1];
                nextCursor = CreateCursor(lastItem.TransactionId, lastItem.CreatedAt);
            }

            logger.LogInformation(
                "판매 내역 조회 성공 - UserId: {UserId}, 조회된 항목 수: {Count}, 전체 수: {TotalCount}",
                userId, items.Count, totalCount?.ToString() ?? "N/A");

            return new TransactionHistoryRespDto
            {
                Items = items,
                NextCursor = nextCursor,
                HasMore = hasMore,
                TotalCount = totalCount
            };
        }
        catch (AppException)
        {
            // AppException은 그대로 전파
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "판매 내역 조회 중 예외 발생 - UserId: {UserId}", userId);
            throw new AppException("판매 내역 조회 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError, ex);
        }
    }

    /// <summary>
    /// status 파라미터 검증
    /// </summary>
    private void ValidateStatusParameter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return; // null 또는 빈 값은 허용 (전체 조회)
        }

        // 쉼표로 구분된 복수 status 검증
        var statuses = status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var invalidStatuses = statuses.Where(s => !ValidStatuses.Contains(s)).ToList();

        if (invalidStatuses.Any())
        {
            logger.LogWarning("유효하지 않은 status 값: {InvalidStatuses}", string.Join(", ", invalidStatuses));
            throw new AppException(
                $"유효하지 않은 status 값입니다: {string.Join(", ", invalidStatuses)}. " +
                $"허용된 값: {string.Join(", ", ValidStatuses)}",
                HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// period 파라미터 검증
    /// </summary>
    private void ValidatePeriodParameter(string? period)
    {
        if (string.IsNullOrWhiteSpace(period))
        {
            return; // null은 허용 (기본값 'all' 사용)
        }

        if (!ValidPeriods.Contains(period))
        {
            logger.LogWarning("유효하지 않은 period 값: {Period}", period);
            throw new AppException(
                $"유효하지 않은 period 값입니다: {period}. " +
                $"허용된 값: {string.Join(", ", ValidPeriods)}",
                HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// cursor 파싱 (Base64 인코딩된 JSON)
    /// </summary>
    private (long? Id, DateTime? CreatedAt) ParseCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return (null, null);
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var data = JsonSerializer.Deserialize<CursorData>(json);

            if (data == null)
            {
                logger.LogWarning("cursor 파싱 실패 - 역직렬화 결과가 null");
                throw new AppException("유효하지 않은 cursor 형식입니다.", HttpStatusCode.BadRequest);
            }

            return (data.Id, data.CreatedAt);
        }
        catch (FormatException ex)
        {
            logger.LogWarning(ex, "cursor Base64 디코딩 실패 - Cursor: {Cursor}", cursor);
            throw new AppException("유효하지 않은 cursor 형식입니다. (Base64 디코딩 실패)", HttpStatusCode.BadRequest, ex);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "cursor JSON 역직렬화 실패 - Cursor: {Cursor}", cursor);
            throw new AppException("유효하지 않은 cursor 형식입니다. (JSON 파싱 실패)", HttpStatusCode.BadRequest, ex);
        }
    }

    private string CreateCursor(long id, DateTime createdAt)
    {
        var data = new CursorData { Id = id, CreatedAt = createdAt };
        var json = JsonSerializer.Serialize(data);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private class CursorData
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
