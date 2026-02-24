using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.DTO.Reputation;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.Disputes;
using TicketPlatFormServer.Repository.Reputation;
using TicketPlatFormServer.Repository.Transactions;

namespace TicketPlatFormServer.Services.Reputation;

public class ReputationService(
    IReputationRepository reputationRepository,
    ITransactionRepository transactionRepository,
    IDisputeRepository disputeRepository,
    TicketContext context) : IReputationService
{
    public async Task<long> CreateAsync(long requestUserId, CreateReputationReqDto dto)
    {
        if (dto.Score is < 1 or > 5)
        {
            throw new AppException("별점은 1점에서 5점 사이여야 합니다.", HttpStatusCode.BadRequest);
        }

        var transaction = await transactionRepository.GetTransactionById(dto.TransactionId);
        if (transaction == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (requestUserId != transaction.BuyerId)
        {
            throw new AppException("구매자만 리뷰 작성이 가능합니다.", HttpStatusCode.Forbidden);
        }

        if (transaction.CancelledAt != null)
        {
            throw new AppException("취소된 거래는 리뷰 작성이 불가능합니다.", HttpStatusCode.BadRequest);
        }

        if (transaction.ConfirmedAt == null)
        {
            throw new AppException("구매 확정 이후에 리뷰 작성이 가능합니다.", HttpStatusCode.BadRequest);
        }

        if ((DateTime.UtcNow - transaction.ConfirmedAt.Value).TotalDays > 7)
        {
            throw new AppException("리뷰 작성 기간이 만료되었습니다.", HttpStatusCode.BadRequest);
        }

        var hasOpenDispute = await HasOpenDisputeAsync(dto.TransactionId);
        if (hasOpenDispute)
        {
            throw new AppException("분쟁이 진행 중인 거래는 리뷰 작성이 불가능합니다.", HttpStatusCode.BadRequest);
        }

        if (await reputationRepository.ExistsAsync(dto.TransactionId, requestUserId))
        {
            throw new AppException("이미 해당 거래에 리뷰를 작성했습니다.", HttpStatusCode.Conflict);
        }

        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            var reputationId = await reputationRepository.InsertAsync(new UserReputation
            {
                UserId = transaction.SellerId,
                ReviewerId = requestUserId,
                TransactionId = dto.TransactionId,
                RatingTypeId = 1,
                Score = dto.Score,
                CreatedAt = DateTime.UtcNow
            });

            var delta = (dto.Score - 3) * 1.0f;
            var affectedRows = await reputationRepository.IncrementUserProfileStatsAsync(transaction.SellerId, delta, dto.Score);
            if (affectedRows == 0)
            {
                throw new AppException("판매자 프로필 통계 갱신에 실패했습니다.", HttpStatusCode.NotFound);
            }

            await dbTransaction.CommitAsync();

            return reputationId;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ReputationListRespDto> GetByUserIdAsync(long targetUserId, int page, int size)
    {
        var actualPage = page <= 0 ? 1 : page;
        var actualSize = size <= 0 ? 20 : Math.Min(size, 100);

        var reputations = await reputationRepository.GetByTargetUserIdAsync(targetUserId, actualPage, actualSize);
        var totalCount = await reputationRepository.CountByTargetUserIdAsync(targetUserId);
        var (averageRating, _) = await reputationRepository.GetUserProfileStatsAsync(targetUserId);

        var reviewerIds = reputations.Select(x => x.ReviewerId).Distinct().ToArray();
        var reviewerProfiles = await reputationRepository.GetReviewerProfilesAsync(reviewerIds);

        var items = reputations.Select(x =>
        {
            var hasProfile = reviewerProfiles.TryGetValue(x.ReviewerId, out var profile);
            return new ReputationRespDto
            {
                Id = x.Id,
                ReviewerNickname = hasProfile ? profile.Nickname : "Unknown",
                ReviewerProfileImageUrl = hasProfile ? profile.ProfileImageUrl : null,
                Score = x.Score,
                CreatedAt = x.CreatedAt ?? DateTime.UtcNow
            };
        }).ToList();

        return new ReputationListRespDto
        {
            Items = items,
            TotalCount = totalCount,
            AverageRating = averageRating.HasValue ? (float)averageRating.Value : null
        };
    }

    public async Task<ReputationCheckRespDto> CheckAsync(long requestUserId, long transactionId)
    {
        var transaction = await transactionRepository.GetTransactionById(transactionId);
        if (transaction == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (requestUserId != transaction.BuyerId)
        {
            throw new AppException("구매자만 리뷰 가능 여부를 확인할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        var hasReviewed = await reputationRepository.ExistsAsync(transactionId, requestUserId);
        var reviewDeadline = transaction.ConfirmedAt?.AddDays(7);
        var isExpired = reviewDeadline.HasValue && reviewDeadline.Value < DateTime.UtcNow;
        var hasOpenDispute = await HasOpenDisputeAsync(transactionId);

        var canReview = transaction.CancelledAt == null
                        && transaction.ConfirmedAt != null
                        && !isExpired
                        && !hasReviewed
                        && !hasOpenDispute;

        return new ReputationCheckRespDto
        {
            CanReview = canReview,
            HasReviewed = hasReviewed,
            ReviewDeadline = isExpired ? null : reviewDeadline
        };
    }

    private async Task<bool> HasOpenDisputeAsync(long transactionId)
    {
        var pendingStatus = await disputeRepository.GetDisputeStatusByCodeAsync("PENDING");
        var inReviewStatus = await disputeRepository.GetDisputeStatusByCodeAsync("IN_REVIEW");

        if (pendingStatus == null || inReviewStatus == null)
        {
            throw new AppException("분쟁 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        return await disputeRepository.HasActiveDisputeAsync(transactionId, [pendingStatus.Id, inReviewStatus.Id]);
    }
}
