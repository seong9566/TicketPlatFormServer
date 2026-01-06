using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.Repository.Events;
using TicketPlatFormServer.Repository.ReadModels;
using TicketPlatFormServer.Repository.Ticket;

namespace TicketPlatFormServer.Services.Event;

/// <summary>
/// 이벤트 관련 Service 구현체
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepo;
    private readonly ITicketRepository _ticketRepo;

    public EventService(IEventRepository eventRepo, ITicketRepository ticketRepo)
    {
        _eventRepo = eventRepo;
        _ticketRepo = ticketRepo;
    }

    public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new AppException(message: "유효하지 않은 카테고리 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        var readModels = await _eventRepo.GetEventsByCategoryId(categoryId);

        // ReadModel → RespDto 변환
        return readModels.Select(rm => new EventListRespDto
        {
            EventId = rm.EventId,
            EventTitle = rm.EventTitle,
            EventPosterImageUrl = rm.EventPosterImageUrl,
            StartAt = rm.StartAt,
            EndAt = rm.EndAt,
            VenueName = rm.VenueName,
            ArtistId = rm.ArtistId,
            ArtistName = rm.ArtistName,
            ArtistProfileImageUrl = rm.ArtistProfileImageUrl,
            EventCreatedAt = rm.EventCreatedAt,
            IsNew = rm.IsNew
        }).ToList();
    }

    public async Task<EventDetailRespDto> GetEventDetailWithTickets(int eventId)
    {
        if (eventId <= 0)
        {
            throw new AppException(message: "유효하지 않은 이벤트 ID입니다.", statusCode: HttpStatusCode.BadRequest);
        }

        // 이벤트 상세 정보 조회
        var eventReadModel = await _eventRepo.GetEventDetailById(eventId);

        if (eventReadModel == null)
        {
            throw new AppException(message: "이벤트를 찾을 수 없습니다.", statusCode: HttpStatusCode.NotFound);
        }

        // 티켓 목록 조회
        var ticketReadModels = await _ticketRepo.GetTicketsByEventId(eventId);

        // 좌석 타입 필터 생성 및 매진 임박 여부 계산
        var seatTypeCounts = new Dictionary<string, int>();
        bool isSoldOutImminent = false;

        foreach (var ticket in ticketReadModels)
        {
            // 좌석 타입별 개수 집계
            if (!string.IsNullOrEmpty(ticket.SeatType))
            {
                if (!seatTypeCounts.ContainsKey(ticket.SeatType))
                {
                    seatTypeCounts[ticket.SeatType] = 0;
                }
                seatTypeCounts[ticket.SeatType]++;
            }

            // 매진 임박 체크 (remaining_quantity가 5개 이하인 티켓이 있는지)
            if (ticket.RemainingQuantity <= 5)
            {
                isSoldOutImminent = true;
            }
        }

        // 좌석 타입 필터 생성
        var seatTypeFilters = new List<SeatTypeFilterDto>();

        // 전체좌석 추가
        seatTypeFilters.Add(new SeatTypeFilterDto
        {
            SeatTypeName = "전체좌석",
            TicketCount = ticketReadModels.Count
        });

        // 각 좌석 타입별 필터 추가
        foreach (var kvp in seatTypeCounts.OrderBy(x => x.Key))
        {
            seatTypeFilters.Add(new SeatTypeFilterDto
            {
                SeatTypeName = kvp.Key,
                TicketCount = kvp.Value
            });
        }

        // ReadModel → RespDto 변환
        return new EventDetailRespDto
        {
            EventId = eventReadModel.EventId,
            EventTitle = eventReadModel.EventTitle,
            EventPosterImageUrl = eventReadModel.EventPosterImageUrl,
            StartAt = eventReadModel.StartAt,
            EndAt = eventReadModel.EndAt,
            VenueName = eventReadModel.VenueName,
            VenueAddress = eventReadModel.VenueAddress,
            ArtistId = eventReadModel.ArtistId,
            ArtistName = eventReadModel.ArtistName,
            IsSoldOutImminent = isSoldOutImminent,
            SeatTypeFilters = seatTypeFilters,
            Tickets = ticketReadModels.Select(tm => new TicketListRespDto
            {
                TicketId = tm.TicketId,
                TicketTitle = tm.TicketTitle,
                SeatInfo = tm.SeatInfo,
                SeatType = tm.SeatType,
                Price = tm.Price,
                OriginalPrice = tm.OriginalPrice,
                SeatFeatures = tm.SeatFeatures,
                Description = tm.Description,
                CreatedAt = tm.CreatedAt,
                Quantity = tm.Quantity,
                RemainingQuantity = tm.RemainingQuantity,
                IsSingleTicket = tm.IsSingleTicket,
                TicketImages = tm.TicketImages,
                Seller = new SellerInfoDto
                {
                    UserId = tm.Seller.UserId,
                    Nickname = tm.Seller.Nickname,
                    ProfileImageUrl = tm.Seller.ProfileImageUrl,
                    MannerTemperature = tm.Seller.MannerTemperature,
                    TotalTradeCount = tm.Seller.TotalTradeCount,
                    ResponseRate = tm.Seller.ResponseRate,
                    IsSecurePayment = tm.Seller.IsSecurePayment
                }
            }).ToList()
        };
    }
}

