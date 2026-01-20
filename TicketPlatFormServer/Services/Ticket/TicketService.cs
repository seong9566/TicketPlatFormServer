using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Ticket;
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

    public async Task<TicketDetailRespDto> GetTicketDetailById(int ticketId, int? userId = null)
    {
        if (ticketId <= 0)
        {
            throw new AppException(message: "유효하지 않은 티켓 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        var readModel = await _repo.GetTicketDetailByIdWithEvent(ticketId);

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

        // Signed URL 변환 대상 키 수집 (티켓 이미지 + 판매자 프로필 이미지 + 이벤트 포스터)
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

        var eventPosterKey = readModel.Event.PosterImageUrl;
        bool shouldSignEventPoster = !string.IsNullOrEmpty(eventPosterKey) && !eventPosterKey.StartsWith("http");

        if (shouldSignEventPoster)
        {
            keysToSign.Add(eventPosterKey!);
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

        // 3. 이벤트 포스터 이미지 URL 매핑
        string? finalEventPosterUrl = eventPosterKey;
        if (shouldSignEventPoster && signedUrls.TryGetValue(eventPosterKey!, out var posterResult))
        {
            finalEventPosterUrl = posterResult.SignedUrl;
        }

        // 4. 특이사항 DTO 변환
        var features = readModel.Features?.Select(f => new TicketFeatureDto
        {
            FeatureId = f.FeatureId,
            Code = f.Code,
            NameKo = f.NameKo
        }).ToList();

        // ReadModel → RespDto 변환
        return new TicketDetailRespDto
        {
            TicketId = readModel.TicketId,
            // 좌석 등급 정보 (확장)
            SeatGradeId = readModel.SeatGradeId,
            SeatGradeCode = readModel.SeatGradeCode,
            SeatGradeName = readModel.SeatGradeName,
            SeatGradeNameEn = readModel.SeatGradeNameEn,
            // 구역 정보 (확장)
            AreaId = readModel.AreaId,
            Area = readModel.Area,
            // 위치 정보 (NEW)
            LocationId = readModel.LocationId,
            LocationName = readModel.LocationName,
            // 기존 필드
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
            Features = features?.Any() == true ? features : null,
            Seller = new SellerInfoDto
            {
                UserId = readModel.Seller.UserId,
                Nickname = readModel.Seller.Nickname,
                ProfileImageUrl = finalSellerProfileUrl,
                MannerTemperature = readModel.Seller.MannerTemperature,
                TotalTradeCount = readModel.Seller.TotalTradeCount,
                ResponseRate = readModel.Seller.ResponseRate,
                IsSecurePayment = readModel.Seller.IsSecurePayment
            },
            Event = new EventInfoDto
            {
                EventId = readModel.Event.EventId,
                EventTitle = readModel.Event.EventTitle,
                PosterImageUrl = finalEventPosterUrl,
                StartAt = readModel.Event.StartAt,
                EndAt = readModel.Event.EndAt,
                VenueName = readModel.Event.VenueName
            }
        };
    }
}

