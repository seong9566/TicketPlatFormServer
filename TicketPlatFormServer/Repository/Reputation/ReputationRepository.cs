using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Reputation;

public class ReputationRepository(TicketContext context, IDbConnection dapper) : IReputationRepository
{
    public async Task<bool> ExistsAsync(long transactionId, long reviewerId)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM user_reputation
            WHERE transaction_id = @TransactionId
              AND reviewer_id = @ReviewerId
        ";

        var count = await ExecuteScalarWithCurrentTransactionAsync<int>(query, new
        {
            TransactionId = transactionId,
            ReviewerId = reviewerId
        });

        return count > 0;
    }

    public async Task<long> InsertAsync(UserReputation reputation)
    {
        reputation.CreatedAt ??= DateTime.UtcNow;
        await context.UserReputations.AddAsync(reputation);
        await context.SaveChangesAsync();
        return reputation.Id;
    }

    public async Task<IReadOnlyList<UserReputation>> GetByTargetUserIdAsync(long userId, int page, int size)
    {
        const string query = @"
            SELECT
                id AS Id,
                user_id AS UserId,
                reviewer_id AS ReviewerId,
                transaction_id AS TransactionId,
                rating_type_id AS RatingTypeId,
                score AS Score,
                created_at AS CreatedAt
            FROM user_reputation
            WHERE user_id = @UserId
            ORDER BY created_at DESC, id DESC
            LIMIT @Offset, @Size
        ";

        var offset = (page - 1) * size;
        var items = await QueryWithCurrentTransactionAsync<UserReputation>(query, new
        {
            UserId = userId,
            Offset = offset,
            Size = size
        });

        return items.ToList();
    }

    public async Task<int> CountByTargetUserIdAsync(long userId)
    {
        const string query = @"
            SELECT COUNT(1)
            FROM user_reputation
            WHERE user_id = @UserId
        ";

        return await ExecuteScalarWithCurrentTransactionAsync<int>(query, new { UserId = userId });
    }

    public async Task<(decimal? AverageRating, int ReviewCount)> GetUserProfileStatsAsync(long userId)
    {
        const string query = @"
            SELECT average_rating AS AverageRating, review_count AS ReviewCount
            FROM user_profile
            WHERE user_id = @UserId
        ";

        var row = await QuerySingleOrDefaultWithCurrentTransactionAsync<UserProfileStatRow>(query, new { UserId = userId });
        return row == null ? (null, 0) : (row.AverageRating, row.ReviewCount);
    }

    public async Task<int> IncrementUserProfileStatsAsync(long userId, float delta, int score)
    {
        const string query = @"
            UPDATE user_profile
            SET manner_temperature = LEAST(100.0, GREATEST(0.0, COALESCE(manner_temperature, 36.5) + @Delta)),
                average_rating = ROUND(((COALESCE(average_rating, 0) * review_count) + @Score) / (review_count + 1), 2),
                review_count = review_count + 1
            WHERE user_id = @UserId
        ";

        return await ExecuteWithCurrentTransactionAsync(query,
            new MySqlParameter("@UserId", userId),
            new MySqlParameter("@Delta", delta),
            new MySqlParameter("@Score", score));
    }

    public async Task<IReadOnlyDictionary<long, (string Nickname, string? ProfileImageUrl)>> GetReviewerProfilesAsync(IReadOnlyCollection<long> reviewerIds)
    {
        if (reviewerIds.Count == 0)
        {
            return new Dictionary<long, (string Nickname, string? ProfileImageUrl)>();
        }

        const string query = @"
            SELECT
                user_id AS UserId,
                nickname AS Nickname,
                profile_image_url AS ProfileImageUrl
            FROM user_profile
            WHERE user_id IN @UserIds
        ";

        var rows = await QueryWithCurrentTransactionAsync<ReviewerProfileRow>(query, new { UserIds = reviewerIds.ToArray() });
        return rows.ToDictionary(
            x => x.UserId,
            x => (x.Nickname ?? "Unknown", x.ProfileImageUrl));
    }

    private async Task<IEnumerable<T>> QueryWithCurrentTransactionAsync<T>(string sql, object? param = null)
    {
        var currentTransaction = context.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            var connection = context.Database.GetDbConnection();
            return await connection.QueryAsync<T>(sql, param, transaction: currentTransaction.GetDbTransaction());
        }

        return await dapper.QueryAsync<T>(sql, param);
    }

    private async Task<T> ExecuteScalarWithCurrentTransactionAsync<T>(string sql, object? param = null)
    {
        var currentTransaction = context.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            var connection = context.Database.GetDbConnection();
            var result = await connection.ExecuteScalarAsync<T>(sql, param, transaction: currentTransaction.GetDbTransaction());
            return result is null ? default! : result;
        }

        var fallbackResult = await dapper.ExecuteScalarAsync<T>(sql, param);
        return fallbackResult is null ? default! : fallbackResult;
    }

    private async Task<T?> QuerySingleOrDefaultWithCurrentTransactionAsync<T>(string sql, object? param = null)
    {
        var currentTransaction = context.Database.CurrentTransaction;
        if (currentTransaction != null)
        {
            var connection = context.Database.GetDbConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction: currentTransaction.GetDbTransaction());
        }

        return await dapper.QuerySingleOrDefaultAsync<T>(sql, param);
    }

    private async Task<int> ExecuteWithCurrentTransactionAsync(string sql, params MySqlParameter[] parameters)
    {
        return await context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    private sealed class UserProfileStatRow
    {
        public decimal? AverageRating { get; init; }
        public int ReviewCount { get; init; }
    }

    private sealed class ReviewerProfileRow
    {
        public long UserId { get; init; }
        public string? Nickname { get; init; }
        public string? ProfileImageUrl { get; init; }
    }
}
