using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Ticket;
using TicketPlatFormServer.Repository.Events;
using TicketPlatFormServer.Repository.Favorite;
using TicketPlatFormServer.Repository.ReadModels;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Services.FileUpload;

namespace TicketPlatFormServer.Services.Event;

/// <summary>
/// 이벤트 관련 Service 구현체
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepo;
    private readonly ITicketRepository _ticketRepo;
    private readonly IFavoriteRepository _favoriteRepo;
    private readonly IFileUploadService _fileUploadService;

    public EventService(
        IEventRepository eventRepo, 
        ITicketRepository ticketRepo, 
        IFavoriteRepository favoriteRepo,
        IFileUploadService fileUploadService)
    {
        _eventRepo = eventRepo;
        _ticketRepo = ticketRepo;
        _favoriteRepo = favoriteRepo;
        _fileUploadService = fileUploadService;
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

    public async Task<EventDetailRespDto> GetEventDetailWithTickets(int eventId, int? userId = null)
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

        // 찜 목록 조회 (userId가 있는 경우)
        HashSet<int> favoritedTicketIds = new HashSet<int>();
        if (userId.HasValue)
        {
            // 찜 타입 
            // Ticket : 2
            // Event : 1
            const int ticketFavoriteTypeId = 2; // 티켓 찜 타입 ID
            foreach (var ticket in ticketReadModels)
            {
                var isFavorited = await _favoriteRepo.CheckIsFavorited(userId.Value, ticketFavoriteTypeId, ticket.TicketId);
                if (isFavorited)
                {
                    favoritedTicketIds.Add(ticket.TicketId);
                }
            }
        }

        // 좌석 등급 필터 생성 및 매진 임박 여부 계산
        var seatGradeCounts = new Dictionary<string, int>();
        var locationCounts = new Dictionary<int, (string name, int count, int sortOrder)>();
        bool isSoldOutImminent = false;

        foreach (var ticket in ticketReadModels)
        {
            // 좌석 등급별 개수 집계
            if (!string.IsNullOrEmpty(ticket.SeatGradeName))
            {
                if (!seatGradeCounts.ContainsKey(ticket.SeatGradeName))
                {
                    seatGradeCounts[ticket.SeatGradeName] = 0;
                }
                seatGradeCounts[ticket.SeatGradeName]++;
            }

            // 위치별 개수 집계 (NEW)
            if (ticket.LocationId.HasValue)
            {
                var locId = ticket.LocationId.Value;
                if (!locationCounts.ContainsKey(locId))
                {
                    locationCounts[locId] = (
                        ticket.LocationName ?? "미분류",
                        0,
                        ticket.LocationSortOrder ?? 999
                    );
                }
                var (name, count, sort) = locationCounts[locId];
                locationCounts[locId] = (name, count + 1, sort);
            }

            // 매진 임박 체크 (remaining_quantity가 5개 이하인 티켓이 있는지)
            if (ticket.RemainingQuantity <= 5)
            {
                isSoldOutImminent = true;
            }
        }

        // 좌석 등급 필터 생성
        var seatTypeFilters = new List<SeatTypeFilterDto>();

        // 전체좌석 추가
        seatTypeFilters.Add(new SeatTypeFilterDto
        {
            SeatTypeName = "전체좌석",
            TicketCount = ticketReadModels.Count
        });

        // 각 좌석 등급별 필터 추가
        foreach (var kvp in seatGradeCounts.OrderBy(x => x.Key))
        {
            seatTypeFilters.Add(new SeatTypeFilterDto
            {
                SeatTypeName = kvp.Key,
                TicketCount = kvp.Value
            });
        }

        // 위치 필터 생성 (NEW)
        var seatLocationFilters = locationCounts
            .OrderBy(x => x.Value.sortOrder)
            .Select(x => new SeatLocationDto
            {
                LocationId = x.Key,
                LocationName = x.Value.name,
                TicketCount = x.Value.count,
                SortOrder = x.Value.sortOrder
            })
            .ToList();

        // 티켓 이미지 object key → Supabase signed URL 변환
        var imageKeysToSign = ticketReadModels
            .SelectMany(t => t.TicketImages)
            .Where(key => !string.IsNullOrEmpty(key) && !key.StartsWith("http"))
            .Distinct()
            .ToList();

        var signedUrlMap = new Dictionary<string, string>();
        if (imageKeysToSign.Count > 0)
        {
            var signedResults = await _fileUploadService.RefreshSignedUrlsBatchAsync(imageKeysToSign);
            foreach (var (key, result) in signedResults)
            {
                signedUrlMap[key] = result.SignedUrl;
            }
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
            SeatLocationFilters = seatLocationFilters, // NEW
            Tickets = ticketReadModels.Select(tm => new TicketListRespDto
            {
                TicketId = tm.TicketId,
                SeatGradeId = tm.SeatGradeId,
                SeatGradeCode = tm.SeatGradeCode, // NEW
                SeatGradeName = tm.SeatGradeName,
                SeatGradeNameEn = tm.SeatGradeNameEn, // NEW
                AreaId = tm.AreaId, // NEW
                Area = tm.Area,
                LocationId = tm.LocationId, // NEW
                LocationName = tm.LocationName, // NEW
                Row = tm.Row,
                Price = tm.Price,
                OriginalPrice = tm.OriginalPrice,
                IsConsecutive = tm.IsConsecutive,
                TradeMethodId = tm.TradeMethodId,
                TradeMethodName = tm.TradeMethodName,
                HasTicket = tm.HasTicket,
                Description = tm.Description,
                CreatedAt = tm.CreatedAt,
                Quantity = tm.Quantity,
                RemainingQuantity = tm.RemainingQuantity,
                IsSingleTicket = tm.IsSingleTicket,
                // 썸네일 이미지 URL 변환: object key → signed URL
                TicketImages = tm.TicketImages
                    .Select(key => signedUrlMap.TryGetValue(key, out var url) ? url : key)
                    .ToList(),
                IsFavorited = userId.HasValue ? favoritedTicketIds.Contains(tm.TicketId) : null,
                Features = tm.Features?.Select(f => new TicketFeatureDto
                {
                    FeatureId = f.FeatureId,
                    Code = f.Code,
                    NameKo = f.NameKo
                }).ToList(),
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
