using System.Globalization;
using System.Net;
using TicketPlatFormServer.DTO.Sell;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Repository.Sell;
using TicketPlatFormServer.Services.Storage;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Services.Sell;

/// <summary>
/// 티켓 판매 Service 구현체
/// </summary>
public class SellService(ISellRepository sellRepository, IStorageUploader storageUploader) : ISellService
{
    private readonly ISellRepository _sellRepository = sellRepository;
    private readonly IStorageUploader _storageUploader = storageUploader;

    /// <summary>
    /// 판매 가능한 카테고리 목록 조회
    /// </summary>
    public async Task<List<CategoryRespDto>> GetCategoriesAsync()
    {
        var categories = await _sellRepository.GetActiveCategoriesAsync();

        return categories.Select(c => new CategoryRespDto
        {
            CategoryId = c.Id,
            Code = c.Code,
            Name = c.NameKo,
            IconUrl = null // TODO: 아이콘 URL 추가 시 매핑
        }).ToList();
    }

    /// <summary>
    /// 카테고리별 공연 목록 조회 (페이징)
    /// </summary>
    public async Task<SellEventListRespDto> GetEventsAsync(SellEventListReqDto request)
    {
        var (events, totalCount) = await _sellRepository.GetEventsByCategoryAsync(
            request.CategoryId,
            request.Keyword,
            request.Page,
            request.Size);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.Size);

        return new SellEventListRespDto
        {
            Events = events.Select(e => new SellEventItem
            {
                EventId = e.Id,
                Title = e.Title,
                PosterImageUrl = e.PosterImageUrl,
                VenueName = e.VenueName,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            }).ToList(),
            TotalCount = totalCount,
            CurrentPage = request.Page,
            PageSize = request.Size,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// 특정 공연의 일정 목록 조회
    /// </summary>
    public async Task<EventScheduleRespDto> GetEventSchedulesAsync(int eventId)
    {
        // 공연 존재 여부 확인
        var eventEntity = await _sellRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new AppException("해당 공연을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        var schedules = await _sellRepository.GetEventSchedulesAsync(eventId);

        return new EventScheduleRespDto
        {
            Schedules = schedules.Select(s => new ScheduleItem
            {
                ScheduleId = s.Id,
                Date = s.ScheduleDate.ToString("yyyy-MM-dd"),
                Time = s.ScheduleTime.ToString("HH:mm"),
                DayOfWeek = GetKoreanDayOfWeek(s.ScheduleDate)
            }).ToList()
        };
    }

    /// <summary>
    /// 특정 공연의 좌석 옵션 조회
    /// </summary>
    public async Task<SeatOptionRespDto> GetSeatOptionsAsync(int eventId)
    {
        // 공연 존재 여부 확인
        var eventEntity = await _sellRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new AppException("해당 공연을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        var locations = await _sellRepository.GetSeatLocationsAsync(eventId);

        return new SeatOptionRespDto
        {
            Locations = locations.Select(l => new SeatLocationOption
            {
                LocationId = l.Id,
                LocationName = l.LocationName
            }).ToList(),
            AllowCustomLocation = true
        };
    }

    /// <summary>
    /// 티켓 판매 등록
    /// </summary>
    public async Task<CreateSellTicketRespDto> CreateTicketAsync(int userId, CreateSellTicketReqDto request)
    {
        // 1. 공연 존재 여부 확인
        var eventEntity = await _sellRepository.GetEventByIdAsync(request.EventId);
        if (eventEntity == null)
        {
            throw new AppException("해당 공연을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 2. 일정 존재 여부 확인
        var schedule = await _sellRepository.GetScheduleByIdAsync(request.ScheduleId);
        if (schedule == null)
        {
            throw new AppException("해당 일정을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 3. 가격 검증
        if (request.Price > request.OriginalPrice)
        {
            throw new AppException("판매가는 정가를 초과할 수 없습니다.", HttpStatusCode.BadRequest);
        }

        // 4. 티켓 제목 생성
        var title = $"{eventEntity.Title} - {schedule.ScheduleDate:yyyy-MM-dd}";

        // 5. pending_review 상태 ID 조회
        var pendingReviewStatusId = await _sellRepository.GetTicketStatusIdByCodeAsync("pending_review");
        if (pendingReviewStatusId == null)
        {
            throw new AppException("티켓 상태를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        // 6. 공연 일시 계산
        var eventDatetime = new DateTime(
            schedule.ScheduleDate.Year,
            schedule.ScheduleDate.Month,
            schedule.ScheduleDate.Day,
            schedule.ScheduleTime.Hour,
            schedule.ScheduleTime.Minute,
            schedule.ScheduleTime.Second);

        // 7. 티켓 엔티티 생성
        var ticket = new DBModel.Ticket
        {
            SellerId = userId,
            EventId = request.EventId,
            ScheduleId = request.ScheduleId,
            CategoryId = eventEntity.CategoryId,
            Title = title,
            EventDatetime = eventDatetime,
            SeatInfo = request.SeatInfo,
            LocationId = request.LocationId,
            Area = request.Area,
            Row = request.Row,
            Quantity = request.Quantity,
            IsConsecutive = request.IsConsecutive,
            RemainingQuantity = request.Quantity,
            Price = request.Price,
            OriginalPrice = request.OriginalPrice,
            Description = request.Description,
            StatusId = pendingReviewStatusId.Value
        };

        // 8. 티켓 저장
        var ticketId = await _sellRepository.CreateTicketAsync(ticket);

        // 9. 이미지 업로드
        if (request.Images != null && request.Images.Any())
        {
            var ticketImages = new List<TicketImage>();

            foreach (var image in request.Images.Take(5)) // 최대 5개
            {
                using var stream = image.OpenReadStream();
                var objectKey = $"tickets/{ticketId}/{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

                await _storageUploader.UploadAsync(
                    stream,
                    objectKey,
                    image.ContentType);

                ticketImages.Add(new TicketImage
                {
                    TicketId = ticketId,
                    ImageUrl = objectKey // 실제 URL은 조회 시 GetSignedUrlAsync로 생성
                });
            }

            await _sellRepository.CreateTicketImagesAsync(ticketImages);
        }

        return new CreateSellTicketRespDto
        {
            TicketId = ticketId,
            Status = "pending_review",
            Message = "티켓 판매 등록이 완료되었습니다. 검수 후 판매가 시작됩니다."
        };
    }

    /// <summary>
    /// 내 판매 티켓 목록 조회
    /// </summary>
    public async Task<MyTicketListRespDto> GetMyTicketsAsync(int userId, MyTicketListReqDto request)
    {
        var (tickets, totalCount) = await _sellRepository.GetMyTicketsAsync(
            userId,
            request.Status,
            request.Page,
            request.Size);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.Size);

        // 티켓 이미지 조회
        var ticketIds = tickets.Select(t => t.Id).ToList();
        var ticketImages = await GetTicketImagesAsync(ticketIds);

        return new MyTicketListRespDto
        {
            Tickets = tickets.Select(t => new MyTicketItem
            {
                TicketId = t.Id,
                Title = t.Title,
                EventDatetime = t.EventDatetime,
                SeatInfo = t.SeatInfo,
                Quantity = t.Quantity,
                RemainingQuantity = t.RemainingQuantity,
                Price = t.Price,
                OriginalPrice = t.OriginalPrice,
                Status = t.Status.NameKo ?? t.Status.Code,
                CreatedAt = t.CreatedAt,
                ThumbnailUrl = ticketImages.ContainsKey(t.Id) ? ticketImages[t.Id] : null
            }).ToList(),
            TotalCount = totalCount,
            CurrentPage = request.Page,
            PageSize = request.Size,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// 티켓 판매 취소
    /// </summary>
    public async Task<CancelSellTicketRespDto> CancelTicketAsync(int userId, int ticketId)
    {
        // 1. 티켓 조회
        var ticket = await _sellRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
        {
            throw new AppException("티켓을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 2. 소유권 확인
        if (ticket.SellerId != userId)
        {
            throw new AppException("티켓을 취소할 권한이 없습니다.", HttpStatusCode.Forbidden);
        }

        // 3. cancelled 상태 ID 조회
        var cancelledStatusId = await _sellRepository.GetTicketStatusIdByCodeAsync("cancelled");
        if (cancelledStatusId == null)
        {
            throw new AppException("티켓 상태를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        // 4. 상태 업데이트
        await _sellRepository.UpdateTicketStatusAsync(ticketId, cancelledStatusId.Value);

        return new CancelSellTicketRespDto
        {
            TicketId = ticketId,
            Status = "cancelled",
            Message = "티켓 판매가 취소되었습니다."
        };
    }

    /// <summary>
    /// 티켓 이미지 조회 (대표 이미지만)
    /// </summary>
    private async Task<Dictionary<int, string>> GetTicketImagesAsync(List<int> ticketIds)
    {
        // TODO: 실제로는 TicketImageRepository를 통해 조회해야 하지만,
        // 여기서는 간단히 빈 딕셔너리 반환
        return new Dictionary<int, string>();
    }

    /// <summary>
    /// 한글 요일 변환
    /// </summary>
    private static string GetKoreanDayOfWeek(DateOnly date)
    {
        var dayOfWeek = date.DayOfWeek;
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => "일",
            DayOfWeek.Monday => "월",
            DayOfWeek.Tuesday => "화",
            DayOfWeek.Wednesday => "수",
            DayOfWeek.Thursday => "목",
            DayOfWeek.Friday => "금",
            DayOfWeek.Saturday => "토",
            _ => ""
        };
    }
}
