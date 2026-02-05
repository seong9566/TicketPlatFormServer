using System.Text;
using System.Text.Json;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Transaction;
using TicketPlatFormServer.Repository.Transactions;

namespace TicketPlatFormServer.Services.Transaction;

public class TransactionService(ITransactionHistoryRepository repository) : ITransactionService
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 50;

    public async Task<TransactionHistoryRespDto> GetPurchaseHistoryAsync(
        int userId,
        string? status,
        string? period,
        string? sortBy,
        string? cursor,
        int? limit)
    {
        var (cursorId, cursorCreatedAt) = ParseCursor(cursor);
        var actualLimit = Math.Min(limit ?? DefaultLimit, MaxLimit);
        var actualSortBy = sortBy ?? "latest";

        if (actualSortBy != "latest" && actualSortBy != "oldest")
        {
            throw new AppException("sortBy는 'latest' 또는 'oldest'만 가능합니다.", System.Net.HttpStatusCode.BadRequest);
        }

        var (items, totalCount) = await repository.GetPurchaseHistoryAsync(
            userId,
            status,
            period,
            actualSortBy,
            cursorId,
            cursorCreatedAt,
            actualLimit + 1
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

        return new TransactionHistoryRespDto
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            TotalCount = totalCount
        };
    }

    public async Task<TransactionHistoryRespDto> GetSalesHistoryAsync(
        int userId,
        string? status,
        string? period,
        string? sortBy,
        string? cursor,
        int? limit)
    {
        var (cursorId, cursorCreatedAt) = ParseCursor(cursor);
        var actualLimit = Math.Min(limit ?? DefaultLimit, MaxLimit);
        var actualSortBy = sortBy ?? "latest";

        if (actualSortBy != "latest" && actualSortBy != "oldest")
        {
            throw new AppException("sortBy는 'latest' 또는 'oldest'만 가능합니다.", System.Net.HttpStatusCode.BadRequest);
        }

        var (items, totalCount) = await repository.GetSalesHistoryAsync(
            userId,
            status,
            period,
            actualSortBy,
            cursorId,
            cursorCreatedAt,
            actualLimit + 1
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

        return new TransactionHistoryRespDto
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore,
            TotalCount = totalCount
        };
    }

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
            return (data?.Id, data?.CreatedAt);
        }
        catch
        {
            return (null, null);
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
