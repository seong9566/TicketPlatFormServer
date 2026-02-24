using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Reputation;

public interface IReputationRepository
{
    Task<bool> ExistsAsync(long transactionId, long reviewerId);
    Task<long> InsertAsync(UserReputation reputation);
    Task<IReadOnlyList<UserReputation>> GetByTargetUserIdAsync(long userId, int page, int size);
    Task<int> CountByTargetUserIdAsync(long userId);
    Task<(decimal? AverageRating, int ReviewCount)> GetUserProfileStatsAsync(long userId);
    Task<int> IncrementUserProfileStatsAsync(long userId, float delta, int score);
    Task<IReadOnlyDictionary<long, (string Nickname, string? ProfileImageUrl)>> GetReviewerProfilesAsync(IReadOnlyCollection<long> reviewerIds);
}
