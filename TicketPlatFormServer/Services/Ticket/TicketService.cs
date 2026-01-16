using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Repository.Favorite;
using TicketPlatFormServer.Repository.ReadModels;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Services.FileUpload;

namespace TicketPlatFormServer.Services.Ticket;

/// <summary>
/// 티켓 관련 Service 구현체
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly IFavoriteRepository _favoriteRepo;
    private readonly IFileUploadService _fileUploadService;
    private const int FAVORITE_TYPE_TICKET = 2;

    public TicketService(
        ITicketRepository repo, 
        IFavoriteRepository favoriteRepo,
        IFileUploadService fileUploadService)
    {
        _repo = repo;
        _favoriteRepo = favoriteRepo;
        _fileUploadService = fileUploadService;
    }

    public async Task<TicketListRespDto> GetTicketDetailById(int ticketId, int? userId = null)
    {
        if (ticketId <= 0)
        {
            throw new AppException(message: "유효하지 않은 티켓 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        var readModel = await _repo.GetTicketDetailById(ticketId);

        if (readModel == null)
        {
            throw new AppException(message: "티켓을 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 찜 여부 확인 (userId가 제공된 경우만)
        bool? isFavorited = null;
        if (userId.HasValue && userId.Value > 0)
        {
            isFavorited = await _favoriteRepo.CheckIsFavorited(userId.Value, FAVORITE_TYPE_TICKET, ticketId);
        }

        // Signed URL 변환 대상 키 수집 (티켓 이미지 + 판매자 프로필 이미지)
        var keysToSign = new List<string>();
        
        if (readModel.TicketImages != null)
        {
            keysToSign.AddRange(readModel.TicketImages);
        }

        var sellerProfileKey = readModel.Seller.ProfileImageUrl;
        bool shouldSignSellerProfile = !string.IsNullOrEmpty(sellerProfileKey) && !sellerProfileKey.StartsWith("http");
        
        if (shouldSignSellerProfile)
        {
            keysToSign.Add(sellerProfileKey!);
        }

        // 배치 요청으로 Signed URL 획득 (캐시 활용)
        var signedUrls = new Dictionary<string, SignedUrlResult>();
        if (keysToSign.Count > 0)
        {
            signedUrls = await _fileUploadService.RefreshSignedUrlsBatchAsync(keysToSign);
        }

        // 1. 티켓 이미지 URL 매핑
        var ticketImages = new List<string>();
        if (readModel.TicketImages != null)
        {
            foreach (var objectKey in readModel.TicketImages)
            {
                if (signedUrls.TryGetValue(objectKey, out var result))
                {
                    ticketImages.Add(result.SignedUrl);
                }
                else
                {
                    ticketImages.Add(objectKey);
                }
            }
        }

        // 2. 판매자 프로필 이미지 URL 매핑
        string? finalSellerProfileUrl = sellerProfileKey;
        if (shouldSignSellerProfile && signedUrls.TryGetValue(sellerProfileKey!, out var sellerResult))
        {
            finalSellerProfileUrl = sellerResult.SignedUrl;
        }

        // ReadModel → RespDto 변환
        return new TicketListRespDto
        {
            TicketId = readModel.TicketId,
            SeatGradeId = readModel.SeatGradeId,
            SeatGradeName = readModel.SeatGradeName,
            Area = readModel.Area,
            Row = readModel.Row,
            Price = readModel.Price,
            OriginalPrice = readModel.OriginalPrice,
            IsConsecutive = readModel.IsConsecutive,
            TradeMethodId = readModel.TradeMethodId,
            TradeMethodName = readModel.TradeMethodName,
            HasTicket = readModel.HasTicket,
            Description = readModel.Description,
            CreatedAt = readModel.CreatedAt,
            Quantity = readModel.Quantity,
            RemainingQuantity = readModel.RemainingQuantity,
            IsSingleTicket = readModel.IsSingleTicket,
            TicketImages = ticketImages,
            IsFavorited = isFavorited,
            Seller = new SellerInfoDto
            {
                UserId = readModel.Seller.UserId,
                Nickname = readModel.Seller.Nickname,
                ProfileImageUrl = finalSellerProfileUrl,
                MannerTemperature = readModel.Seller.MannerTemperature,
                TotalTradeCount = readModel.Seller.TotalTradeCount,
                ResponseRate = readModel.Seller.ResponseRate,
                IsSecurePayment = readModel.Seller.IsSecurePayment
            }
        };
    }
}
