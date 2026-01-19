using System.Globalization;
using System.Net;
using TicketPlatFormServer.DTO.Sell;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Repository.Sell;
using TicketPlatFormServer.Services.Storage;
using TicketPlatFormServer.Services.FileUpload;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Services.Sell;

/// <summary>
/// 티켓 판매 Service 구현체
/// </summary>
public class SellService(ISellRepository sellRepository, IStorageUploader storageUploader, IFileUploadService fileUploadService) : ISellService
{
    private readonly ISellRepository _sellRepository = sellRepository;
    private readonly IStorageUploader _storageUploader = storageUploader;
    private readonly IFileUploadService _fileUploadService = fileUploadService;

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

        // 6. 티켓 엔티티 생성
        var ticket = new DBModel.Ticket
        {
            SellerId = userId,
            EventId = request.EventId,
            ScheduleId = request.ScheduleId,
            CategoryId = eventEntity.CategoryId,
            EventDatetime = eventDatetime,
            SeatGradeId = request.SeatGradeId,
            SeatLocationId = request.LocationId,  // LocationId → SeatLocationId
            AreaId = request.AreaId,  // Area string → AreaId FK
            Row = request.Row,
            Quantity = request.Quantity,
            IsConsecutive = request.IsConsecutive,
            RemainingQuantity = request.Quantity,
            Price = request.Price,
            // OriginalPrice는 event_seat_prices 테이블에서 조회
            TradeMethodId = request.TradeMethodId,
            HasTicket = request.HasTicket,
            Description = request.Description,
            StatusId = pendingReviewStatusId.Value
        };

        // 8. 티켓 저장
        var ticketId = await _sellRepository.CreateTicketAsync(ticket);

        // 9. 이미지 업로드
        List<TicketImageDto>? uploadedImages = null;
        if (request.Images != null && request.Images.Any())
        {
            // FileUploadService를 통해 배치 업로드 (검증 포함)
            var uploadResults = await _fileUploadService.UploadTicketImagesAsync(
                request.Images,
                ticketId,
                userId);

            // DB에 저장 (object key만)
            var ticketImages = uploadResults.Select(r => new TicketImage
            {
                TicketId = ticketId,
                ImageUrl = r.ObjectKey
            }).ToList();
            await _sellRepository.CreateTicketImagesAsync(ticketImages);

            // DB insert 후 실제 Id를 가져와 매핑
            var savedImages = await _sellRepository.GetTicketImagesByTicketIdAsync(ticketId);
            uploadedImages = uploadResults.Zip(savedImages, (upload, saved) => new TicketImageDto
            {
                ImageId = saved.Id,
                ImageUrl = upload.SignedUrl,
                ExpiresAt = upload.ExpiresAt
            }).ToList();
        }

        return new CreateSellTicketRespDto
        {
            TicketId = ticketId,
            Status = "pending_review",
            Message = "티켓 판매 등록이 완료되었습니다. 검수 후 판매가 시작됩니다.",
            Images = uploadedImages
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
                // Title 필드가 제거되었으므로 Event Title 사용
                Title = t.Event?.Title ?? "",
                EventDatetime = t.EventDatetime,
                SeatGradeName = t.SeatGrade?.NameKo,
                Area = t.Area?.AreaName,  // Area string → EventSeatArea.AreaName
                Row = t.Row,
                Quantity = t.Quantity,
                RemainingQuantity = t.RemainingQuantity,
                Price = t.Price,
                // OriginalPrice는 event_seat_prices에서 조회 필요
                OriginalPrice = 0,  // TODO: event_seat_prices JOIN 또는 별도 조회
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
    /// 티켓 이미지 조회 (대표 이미지만 - 썸네일용)
    /// </summary>
    private async Task<Dictionary<int, string>> GetTicketImagesAsync(List<int> ticketIds)
    {
        if (!ticketIds.Any())
            return new Dictionary<int, string>();

        // 1. DB에서 이미지 조회 (배치)
        var ticketImagesDict = await _sellRepository.GetTicketImagesByTicketIdsAsync(ticketIds);

        // 2. 첫 번째 이미지의 object key만 추출
        var firstImageKeys = ticketImagesDict
            .Where(kvp => kvp.Value.Any())
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.First().ImageUrl);

        if (!firstImageKeys.Any())
            return new Dictionary<int, string>();

        // 3. Signed URL 배치 생성 (FileUploadService 사용)
        var signedUrls = await _fileUploadService.RefreshSignedUrlsBatchAsync(
            firstImageKeys.Values);

        // 4. TicketId -> SignedUrl 매핑
        var result = new Dictionary<int, string>();
        foreach (var (ticketId, objectKey) in firstImageKeys)
        {
            if (signedUrls.TryGetValue(objectKey, out var urlResult))
            {
                result[ticketId] = urlResult.SignedUrl;
            }
        }

        return result;
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

    /// <summary>
    /// 티켓 이미지 URL 재발급
    /// </summary>
    public async Task<RefreshTicketImageUrlRespDto> RefreshTicketImageUrlsAsync(int ticketId, int userId)
    {
        // 1. 티켓 조회 및 소유권 확인
        var ticket = await _sellRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
        {
            throw new AppException("티켓을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (ticket.SellerId != userId)
        {
            throw new AppException("티켓에 접근할 권한이 없습니다.", HttpStatusCode.Forbidden);
        }

        // 2. 이미지 조회
        var images = await _sellRepository.GetTicketImagesByTicketIdAsync(ticketId);
        if (!images.Any())
        {
            return new RefreshTicketImageUrlRespDto { Images = new() };
        }

        // 3. Signed URL 배치 생성
        var objectKeys = images.Select(img => img.ImageUrl).ToList();
        var signedUrls = await _fileUploadService.RefreshSignedUrlsBatchAsync(objectKeys);

        // 4. DTO 매핑
        var imageDtos = images.Select(img =>
        {
            signedUrls.TryGetValue(img.ImageUrl, out var urlResult);
            return new TicketImageDto
            {
                ImageId = img.Id,
                ImageUrl = urlResult?.SignedUrl ?? "",
                ExpiresAt = urlResult?.ExpiresAt ?? DateTime.UtcNow
            };
        }).ToList();

        return new RefreshTicketImageUrlRespDto { Images = imageDtos };
    }
}
