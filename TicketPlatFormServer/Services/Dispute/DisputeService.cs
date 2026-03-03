using System.Net;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Dispute;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.Disputes;
using TicketPlatFormServer.Repository.Payment;
using TicketPlatFormServer.Repository.Transactions;
using TicketPlatFormServer.Services.FileUpload;
using TicketPlatFormServer.Services.Notification;

namespace TicketPlatFormServer.Services.Dispute;

public class DisputeService(
    IDisputeRepository disputeRepository,
    ITransactionRepository transactionRepository,
    IPaymentRepository paymentRepository,
    IFileUploadService fileUploadService,
    INotificationService notificationService,
    TicketContext context,
    ILogger<DisputeService> logger) : IDisputeService
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 50;
    private const int MaxEvidenceCount = 5;

    public async Task<DisputeSummaryRespDto> CreateDisputeAsync(long userId, CreateDisputeReqDto req)
    {
        await disputeRepository.SeedMasterDataIfMissingAsync();

        var transaction = await transactionRepository.GetTransactionById(req.TransactionId);
        if (transaction == null)
        {
            throw new AppException("거래가 존재하지 않습니다.", HttpStatusCode.NotFound);
        }

        if (transaction.BuyerId != userId)
        {
            throw new AppException("해당 거래의 구매자만 신고할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        if (!string.Equals(transaction.Status.Code, "paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("신고 가능한 거래 상태가 아닙니다.", HttpStatusCode.BadRequest);
        }

        var typeCode = req.TypeCode.Trim().ToUpperInvariant();
        var disputeType = await disputeRepository.GetDisputeTypeByCodeAsync(typeCode);
        if (disputeType == null)
        {
            throw new AppException("유효하지 않은 신고 유형입니다.", HttpStatusCode.BadRequest);
        }

        var pendingStatus = await disputeRepository.GetDisputeStatusByCodeAsync("PENDING");
        var inReviewStatus = await disputeRepository.GetDisputeStatusByCodeAsync("IN_REVIEW");
        if (pendingStatus == null || inReviewStatus == null)
        {
            throw new AppException("신고 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        var hasActiveDispute = await disputeRepository.HasActiveDisputeAsync(
            req.TransactionId,
            [pendingStatus.Id, inReviewStatus.Id]);

        if (hasActiveDispute)
        {
            throw new AppException("해당 거래에 이미 처리 중인 신고가 있습니다.", HttpStatusCode.Conflict);
        }

        var escrow = await paymentRepository.GetEscrowByTransactionIdAsync(req.TransactionId);
        if (escrow == null)
        {
            throw new AppException("에스크로 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        var holdingStatus = await paymentRepository.GetEscrowStatusByCodeAsync("holding");
        var frozenStatus = await paymentRepository.GetEscrowStatusByCodeAsync("frozen");
        if (holdingStatus == null || frozenStatus == null)
        {
            throw new AppException("에스크로 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        if (escrow.StatusId != holdingStatus.Id)
        {
            throw new AppException("에스크로 동결이 가능한 상태가 아닙니다.", HttpStatusCode.Conflict);
        }

        await using var dbTransaction = await context.Database.BeginTransactionAsync();

        try
        {
            var dispute = await disputeRepository.CreateDisputeAsync(new DBModel.Dispute
            {
                TransactionId = req.TransactionId,
                ClaimantId = userId,
                TypeId = disputeType.Id,
                Description = req.Description.Trim(),
                StatusId = pendingStatus.Id,
                CreatedAt = DateTime.UtcNow
            });

            escrow.StatusId = frozenStatus.Id;
            escrow.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            try
            {
                await notificationService.CreateAndSendAsync(
                    transaction.SellerId,
                    "DISPUTE_OPENED",
                    "신고가 접수되었습니다",
                    "거래에 대한 신고가 접수되어 확인이 필요합니다.",
                    new Dictionary<string, string>
                    {
                        ["type"] = "DISPUTE_OPENED",
                        ["transactionId"] = req.TransactionId.ToString(),
                        ["disputeId"] = dispute.Id.ToString()
                    });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[DisputeService.CreateDisputeAsync] 신고 알림 발송 실패 - DisputeId={DisputeId}", dispute.Id);
            }

            return new DisputeSummaryRespDto
            {
                Id = dispute.Id,
                TransactionId = dispute.TransactionId,
                TypeCode = disputeType.Code,
                TypeName = disputeType.NameKo ?? disputeType.Code,
                StatusCode = pendingStatus.Code,
                StatusName = pendingStatus.NameKo ?? pendingStatus.Code,
                Description = dispute.Description ?? string.Empty,
                EvidenceCount = 0,
                CreatedAt = dispute.CreatedAt ?? DateTime.UtcNow
            };
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<DisputeListRespDto> GetMyDisputesAsync(long userId, string? cursor, int? limit)
    {
        await disputeRepository.SeedMasterDataIfMissingAsync();

        long? cursorId = null;
        if (!string.IsNullOrWhiteSpace(cursor) && !long.TryParse(cursor, out var parsedCursor))
        {
            throw new AppException("유효하지 않은 cursor 형식입니다.", HttpStatusCode.BadRequest);
        }

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            cursorId = long.Parse(cursor!);
        }

        var actualLimit = Math.Min(limit ?? DefaultLimit, MaxLimit);
        var disputes = await disputeRepository.GetDisputesByClaimantCursorAsync(userId, cursorId, actualLimit + 1);
        var hasMore = disputes.Count > actualLimit;

        if (hasMore)
        {
            disputes = disputes.Take(actualLimit).ToList();
        }

        var disputeIds = disputes.Select(x => x.Id).ToList();
        var evidenceCountMap = await disputeRepository.GetEvidenceCountMapAsync(disputeIds);

        var items = disputes.Select(x => new DisputeSummaryRespDto
        {
            Id = x.Id,
            TransactionId = x.TransactionId,
            TypeCode = x.Type.Code,
            TypeName = x.Type.NameKo ?? x.Type.Code,
            StatusCode = x.Status.Code,
            StatusName = x.Status.NameKo ?? x.Status.Code,
            Description = x.Description ?? string.Empty,
            EvidenceCount = evidenceCountMap.GetValueOrDefault(x.Id, 0),
            CreatedAt = x.CreatedAt ?? DateTime.UtcNow
        }).ToList();

        var nextCursor = hasMore && items.Count > 0 ? items[^1].Id.ToString() : null;

        return new DisputeListRespDto
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    public async Task<DisputeDetailRespDto> GetDisputeDetailAsync(long userId, long disputeId)
    {
        var dispute = await disputeRepository.GetDisputeByIdWithDetailsAsync(disputeId);
        if (dispute == null)
        {
            throw new AppException("신고가 존재하지 않습니다.", HttpStatusCode.NotFound);
        }

        var isClaimant = dispute.ClaimantId == userId;
        var isBuyer = dispute.Transaction.BuyerId == userId;
        var isSeller = dispute.Transaction.SellerId == userId;
        if (!isClaimant && !isBuyer && !isSeller)
        {
            throw new AppException("본인의 신고가 아닙니다.", HttpStatusCode.Forbidden);
        }

        var userIds = new[] { (int)dispute.Transaction.BuyerId, (int)dispute.Transaction.SellerId };
        var nicknameMap = await context.UserProfiles
            .Where(x => userIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.Nickname);

        var firstTicketId = dispute.Transaction.TransactionItems
            .Select(x => x.TicketId)
            .FirstOrDefault();
        var ticketTitle = "티켓 정보 없음";
        if (firstTicketId > 0)
        {
            ticketTitle = await context.Tickets
                .Where(x => x.Id == firstTicketId)
                .Include(x => x.Event)
                .Select(x => x.Event != null ? x.Event.Title : null)
                .FirstOrDefaultAsync()
                ?? "티켓 정보 없음";
        }

        var amount = dispute.Transaction.Amount
            ?? dispute.Transaction.TransactionItems.Sum(x => x.TotalPrice);

        var evidenceList = new List<DisputeEvidenceRespDto>();
        foreach (var evidence in dispute.DisputeEvidences.OrderByDescending(x => x.Id))
        {
            evidenceList.Add(new DisputeEvidenceRespDto
            {
                Id = evidence.Id,
                ImageUrl = await ResolveEvidenceUrlAsync(evidence.ImageUrl),
                Note = evidence.Note,
                CreatedAt = evidence.CreatedAt ?? DateTime.UtcNow
            });
        }

        return new DisputeDetailRespDto
        {
            Id = dispute.Id,
            TransactionId = dispute.TransactionId,
            TypeCode = dispute.Type.Code,
            TypeName = dispute.Type.NameKo ?? dispute.Type.Code,
            StatusCode = dispute.Status.Code,
            StatusName = dispute.Status.NameKo ?? dispute.Status.Code,
            Description = dispute.Description ?? string.Empty,
            Evidences = evidenceList,
            Transaction = new DisputeTransactionRespDto
            {
                TransactionId = dispute.TransactionId,
                TicketTitle = ticketTitle,
                Amount = amount,
                BuyerNickname = nicknameMap.GetValueOrDefault((int)dispute.Transaction.BuyerId) ?? "구매자",
                SellerNickname = nicknameMap.GetValueOrDefault((int)dispute.Transaction.SellerId) ?? "판매자"
            },
            CreatedAt = dispute.CreatedAt ?? DateTime.UtcNow
        };
    }

    public async Task<AddDisputeEvidenceRespDto> AddEvidenceAsync(long userId, long disputeId, AddDisputeEvidenceReqDto req)
    {
        if (req.File == null || req.File.Length == 0)
        {
            throw new AppException("증거 파일이 비어 있습니다.", HttpStatusCode.BadRequest);
        }

        var dispute = await disputeRepository.GetDisputeByIdWithDetailsAsync(disputeId);
        if (dispute == null)
        {
            throw new AppException("신고가 존재하지 않습니다.", HttpStatusCode.NotFound);
        }

        if (dispute.ClaimantId != userId)
        {
            throw new AppException("본인의 신고만 증거를 첨부할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        var statusCode = dispute.Status.Code;
        if (statusCode != "PENDING" && statusCode != "IN_REVIEW")
        {
            throw new AppException("종료된 신고에는 증거를 첨부할 수 없습니다.", HttpStatusCode.Conflict);
        }

        if (dispute.DisputeEvidences.Count >= MaxEvidenceCount)
        {
            throw new AppException($"신고당 증거는 최대 {MaxEvidenceCount}건까지 첨부할 수 있습니다.", HttpStatusCode.BadRequest);
        }

        var uploadResult = await fileUploadService.UploadDisputeEvidenceAsync(req.File, disputeId, userId);

        try
        {
            var evidence = await disputeRepository.CreateEvidenceAsync(new DBModel.DisputeEvidence
            {
                DisputeId = disputeId,
                ImageUrl = uploadResult.ObjectKey,
                Note = req.Note,
                CreatedAt = DateTime.UtcNow
            });

            return new AddDisputeEvidenceRespDto
            {
                Id = evidence.Id,
                DisputeId = evidence.DisputeId,
                ImageUrl = uploadResult.SignedUrl,
                Note = evidence.Note,
                CreatedAt = evidence.CreatedAt ?? DateTime.UtcNow
            };
        }
        catch
        {
            await fileUploadService.DeleteFileAsync(uploadResult.ObjectKey);
            throw;
        }
    }

    public async Task<CancelDisputeRespDto> CancelDisputeAsync(long userId, long disputeId)
    {
        await disputeRepository.SeedMasterDataIfMissingAsync();

        var dispute = await disputeRepository.GetDisputeByIdWithDetailsAsync(disputeId);
        if (dispute == null)
        {
            throw new AppException("신고가 존재하지 않습니다.", HttpStatusCode.NotFound);
        }

        if (dispute.ClaimantId != userId)
        {
            throw new AppException("본인의 신고만 취소할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        var pendingStatus = await disputeRepository.GetDisputeStatusByCodeAsync("PENDING");
        var cancelledStatus = await disputeRepository.GetDisputeStatusByCodeAsync("CANCELLED");
        if (pendingStatus == null || cancelledStatus == null)
        {
            throw new AppException("신고 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        if (dispute.StatusId != pendingStatus.Id)
        {
            throw new AppException("검토 중이거나 종료된 신고는 취소할 수 없습니다.", HttpStatusCode.Conflict);
        }

        var escrow = await paymentRepository.GetEscrowByTransactionIdAsync(dispute.TransactionId);
        if (escrow == null)
        {
            throw new AppException("에스크로 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        var frozenStatus = await paymentRepository.GetEscrowStatusByCodeAsync("frozen");
        var holdingStatus = await paymentRepository.GetEscrowStatusByCodeAsync("holding");
        if (frozenStatus == null || holdingStatus == null)
        {
            throw new AppException("에스크로 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        if (escrow.StatusId != frozenStatus.Id)
        {
            throw new AppException("동결 상태의 에스크로만 복원할 수 있습니다.", HttpStatusCode.Conflict);
        }

        await using var dbTransaction = await context.Database.BeginTransactionAsync();
        try
        {
            await disputeRepository.UpdateDisputeStatusAsync(disputeId, cancelledStatus.Id);

            escrow.StatusId = holdingStatus.Id;
            escrow.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }

        return new CancelDisputeRespDto
        {
            Id = disputeId,
            StatusCode = cancelledStatus.Code,
            StatusName = cancelledStatus.NameKo ?? cancelledStatus.Code
        };
    }

    public async Task<AdminResolveDisputeRespDto> ResolveDisputeAsync(long adminUserId, long disputeId, AdminResolveDisputeReqDto req)
    {
        throw new NotImplementedException();
    }

    public async Task<AdminDisputeListRespDto> GetAllDisputesAsync(string? statusFilter, string? cursor, int? limit)
    {
        throw new NotImplementedException();
    }

    public async Task<DisputeDetailRespDto> GetDisputeDetailForAdminAsync(long disputeId)
    {
        throw new NotImplementedException();
    }

    private async Task<string?> ResolveEvidenceUrlAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
        {
            return imageUrl;
        }

        try
        {
            var signed = await fileUploadService.RefreshSignedUrlAsync(imageUrl);
            return signed.SignedUrl;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[DisputeService.ResolveEvidenceUrlAsync] Signed URL 발급 실패 - ObjectKey={ObjectKey}", imageUrl);
            return imageUrl;
        }
    }
}
