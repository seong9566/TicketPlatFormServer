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

        // 티켓 이미지 URL 변환 (Signed URL)
        var ticketImages = new List<string>();
        if (readModel.TicketImages != null && readModel.TicketImages.Count > 0)
        {
            // 배치 요청으로 Signed URL 획득 (캐시 활용)
            var signedUrls = await _fileUploadService.RefreshSignedUrlsBatchAsync(readModel.TicketImages);
            
            // 순서 보장하며 URL 매핑
            foreach (var objectKey in readModel.TicketImages)
            {
                if (signedUrls.TryGetValue(objectKey, out var result))
                {
                    ticketImages.Add(result.SignedUrl);
                }
                else
                {
                    // 변환 실패 시 원본 키 반환 (또는 제외)
                    ticketImages.Add(objectKey);
                }
            }
        }

        // ReadModel → RespDto 변환
        return new TicketListRespDto
        {
            TicketId = readModel.TicketId,
            TicketTitle = readModel.TicketTitle,
            SeatInfo = readModel.SeatInfo,
            SeatType = readModel.SeatType,
            Price = readModel.Price,
            OriginalPrice = readModel.OriginalPrice,
            SeatFeatures = readModel.SeatFeatures,
            Description = readModel.Description,
            EventTitle = readModel.EventTitle,
            EventDate = readModel.EventDate,
            VenueName = readModel.VenueName,
            EventPosterImageUrl = readModel.EventPosterImageUrl,
            CreatedAt = readModel.CreatedAt,
            Quantity = readModel.Quantity,
            RemainingQuantity = readModel.RemainingQuantity,
            IsSingleTicket = readModel.IsSingleTicket,
            TicketImages = ticketImages, // 변환된 Signed URL 리스트 사용
            IsFavorited = isFavorited,
            Seller = new SellerInfoDto
            {
                UserId = readModel.Seller.UserId,
                Nickname = readModel.Seller.Nickname,
                ProfileImageUrl = readModel.Seller.ProfileImageUrl,
                MannerTemperature = readModel.Seller.MannerTemperature,
                TotalTradeCount = readModel.Seller.TotalTradeCount,
                ResponseRate = readModel.Seller.ResponseRate,
                IsSecurePayment = readModel.Seller.IsSecurePayment
            }
        };
    }
}
