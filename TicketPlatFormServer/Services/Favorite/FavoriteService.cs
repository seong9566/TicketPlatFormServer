using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Favorite;
using TicketPlatFormServer.DTO.Ticket;
using TicketPlatFormServer.Repository.Favorite;

namespace TicketPlatFormServer.Services.Favorite;

/// <summary>
/// 찜 관련 Service 구현체
/// </summary>
public class FavoriteService(IFavoriteRepository repo) : IFavoriteService
{
    private const int FAVORITE_TYPE_TICKET = 2; // favorite_types 테이블의 ticket 타입 ID

    /// <summary>
    /// 티켓 찜 토글 (추가/삭제)
    /// </summary>
    public async Task<FavoriteToggleRespDto> ToggleTicketFavorite(FavoriteToggleReqDto req)
    {
        // 입력 검증
        if (req.UserId <= 0)
        {
            throw new AppException(message: "유효하지 않은 사용자 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        if (req.TicketId <= 0)
        {
            throw new AppException(message: "유효하지 않은 티켓 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        // 티켓 존재 여부 및 판매 가능 상태 확인
        var ticketExists = await repo.CheckTicketExists(req.TicketId);
        if (!ticketExists)
        {
            throw new AppException(message: "티켓을 찾을 수 없거나 판매가 중단되었습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 찜 토글 수행
        var isFavorited = await repo.ToggleFavorite(req.UserId, FAVORITE_TYPE_TICKET, req.TicketId);

        return new FavoriteToggleRespDto
        {
            TicketId = req.TicketId,
            IsFavorited = isFavorited
        };
    }

    /// <summary>
    /// 사용자가 찜한 티켓 목록 조회
    /// </summary>
    public async Task<List<FavoriteTicketListRespDto>> GetFavoriteTicketsByUserId(int userId)
    {
        if (userId <= 0)
        {
            throw new AppException(message: "유효하지 않은 사용자 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        var readModels = await repo.GetFavoriteTicketsByUserId(userId, FAVORITE_TYPE_TICKET);

        // ReadModel → RespDto 변환
        return readModels.Select(rm => new FavoriteTicketListRespDto
        {
            TicketId = rm.TicketId,
            SeatGradeId = rm.SeatGradeId,
            SeatGradeName = rm.SeatGradeName,
            Area = rm.Area,
            Row = rm.Row,
            Price = rm.Price,
            OriginalPrice = rm.OriginalPrice,
            RemainingQuantity = rm.RemainingQuantity,
            IsConsecutive = rm.IsConsecutive,
            TradeMethodId = rm.TradeMethodId,
            TradeMethodName = rm.TradeMethodName,
            HasTicket = rm.HasTicket,
            CreatedAt = rm.CreatedAt,
            FavoritedAt = rm.FavoritedAt,
            EventTitle = rm.EventTitle,
            EventDate = rm.EventDate,
            VenueName = rm.VenueName,
            EventPosterImageUrl = rm.EventPosterImageUrl,
            Seller = new SellerInfoDto
            {
                UserId = rm.Seller.UserId,
                Nickname = rm.Seller.Nickname,
                ProfileImageUrl = rm.Seller.ProfileImageUrl,
                MannerTemperature = rm.Seller.MannerTemperature,
                TotalTradeCount = rm.Seller.TotalTradeCount,
                ResponseRate = rm.Seller.ResponseRate,
                IsSecurePayment = rm.Seller.IsSecurePayment
            }
        }).ToList();
    }
}
