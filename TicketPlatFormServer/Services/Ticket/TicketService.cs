using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Repository.Favorite;
using TicketPlatFormServer.Repository.ReadModels;
using TicketPlatFormServer.Repository.Ticket;

namespace TicketPlatFormServer.Services.Ticket;

/// <summary>
/// 티켓 관련 Service 구현체
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly IFavoriteRepository _favoriteRepo;
    private const int FAVORITE_TYPE_TICKET = 2;

    public TicketService(ITicketRepository repo, IFavoriteRepository favoriteRepo)
    {
        _repo = repo;
        _favoriteRepo = favoriteRepo;
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
            TicketImages = readModel.TicketImages,
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
