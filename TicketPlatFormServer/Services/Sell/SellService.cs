using System.Globalization;
using System.Net;
using TicketPlatFormServer.DTO.Sell;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.ReadModels;
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

    // 티켓 상태 코드 상수
    private const int SALE_STATUS_ID = 1; // 판매 중

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
    /// 특정 공연의 좌석 옵션 조회 (등급, 위치, 구역)
    /// </summary>
    public async Task<SeatOptionRespDto> GetSeatOptionsAsync(int eventId)
    {
        // 공연 존재 여부 확인
        var eventEntity = await _sellRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new AppException("해당 공연을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 좌석 등급 조회 (정가 포함)
        var grades = await _sellRepository.GetSeatGradesAsync(eventId);

        // 좌석 위치 조회
        var locations = await _sellRepository.GetSeatLocationsAsync(eventId);

        // 좌석 구역 조회
        var areas = await _sellRepository.GetSeatAreasAsync(eventId);

        return new SeatOptionRespDto
        {
            Grades = grades.Select(g => new SeatGradeOption
            {
                GradeId = g.Grade.Id,
                Code = g.Grade.Code,
                GradeName = g.Grade.NameKo
            }).ToList(),
            Locations = locations.Select(l => new SeatLocationOption
            {
                LocationId = l.Id,
                LocationName = l.LocationName
            }).ToList(),
            Areas = areas.Select(a => new SeatAreaOption
            {
                AreaId = a.Id,
                AreaName = a.AreaName
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

        // 2.1 일정-공연 매칭 검증 (Codex 이슈 #3)
        if (schedule.EventId != request.EventId)
        {
            throw new AppException("해당 일정은 선택한 공연에 속하지 않습니다.", HttpStatusCode.BadRequest);
        }

        // 3. 정가 조회 및 필수 검증 (Codex 이슈 #2)
        var seatPrice = await _sellRepository.GetSeatPriceAsync(request.EventId, request.SeatGradeId);
        if (seatPrice == null)
        {
            throw new AppException("해당 공연의 좌석 등급에 대한 정가 정보를 찾을 수 없습니다.", HttpStatusCode.BadRequest);
        }

        if (request.Quantity <= 0)
        {
            throw new AppException("수량은 1 이상이어야 합니다.", HttpStatusCode.BadRequest);
        }
        var unitPrice = request.Price / request.Quantity;

        if (unitPrice > seatPrice.OriginalPrice)
        {
            throw new AppException("장당 판매가는 정가를 초과할 수 없습니다.", HttpStatusCode.BadRequest);
        }

        // 5. 공연 일시 계산
        var eventDatetime = new DateTime(
            schedule.ScheduleDate.Year,
            schedule.ScheduleDate.Month,
            schedule.ScheduleDate.Day,
            schedule.ScheduleTime.Hour,
            schedule.ScheduleTime.Minute,
            schedule.ScheduleTime.Second);

        // 6. 트랜잭션으로 티켓 생성, 이미지 저장
        int ticketId = 0;
        List<TicketImageDto>? uploadedImages = null;
        List<string> uploadedObjectKeys = new(); // 롤백용

        using (var transaction = await _sellRepository.BeginTransactionAsync())
        {
            try
            {
                // 6.1 티켓 기본 정보 저장
                var ticket = new DBModel.Ticket
                {
                    SellerId = userId,
                    EventId = request.EventId,
                    ScheduleId = request.ScheduleId,
                    SeatGradeId = request.SeatGradeId,
                    SeatLocationId = request.LocationId,
                    AreaId = request.AreaId,
                    CategoryId = eventEntity.CategoryId,
                    Row = request.Row,
                    Quantity = request.Quantity,
                    RemainingQuantity = request.Quantity,
                    Price = unitPrice,
                    TradeMethodId = request.TradeMethodId,
                    IsConsecutive = request.IsConsecutive,
                    HasTicket = request.HasTicket,
                    Description = request.Description,
                    EventDatetime = schedule.ScheduleDate.ToDateTime(schedule.ScheduleTime),
                    StatusId = SALE_STATUS_ID,
                    FeatureIds = (request.FeatureIds != null && request.FeatureIds.Any())
                        ? string.Join(",", request.FeatureIds)
                        : null
                };

                ticketId = await _sellRepository.CreateTicketAsync(ticket);

                if (request.Images != null && request.Images.Any())
                {
                    try
                    {
                        // FileUploadService를 통해 배치 업로드 (검증 포함)
                        var uploadResults = await _fileUploadService.UploadTicketImagesAsync(
                            request.Images,
                            ticketId,
                            userId);

                        // 업로드된 ObjectKey 추적 (롤백용)
                        uploadedObjectKeys = uploadResults.Select(r => r.ObjectKey).ToList();

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
                    catch
                    {
                        // 이미지 업로드 실패 시 스토리지 롤백
                        foreach (var objectKey in uploadedObjectKeys)
                        {
                            await _fileUploadService.DeleteFileAsync(objectKey);
                        }
                        throw;
                    }
                }

                // 트랜잭션 커밋
                await transaction.CommitAsync();
            }
            catch
            {
                // DB 실패 시 롤백 + 스토리지 정리
                await transaction.RollbackAsync();
                foreach (var objectKey in uploadedObjectKeys)
                {
                    await _fileUploadService.DeleteFileAsync(objectKey);
                }
                throw;
            }
        }

        return new CreateSellTicketRespDto
        {
            TicketId = ticketId,
            Status = "available",
            Message = "티켓 판매 등록이 완료되었습니다.",
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
                Price = t.Price * t.Quantity,
                OriginalPrice = t.SeatGrade?.OriginalPrice ?? 0,
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
    /// 판매 대시보드 조회 (공연별 그룹화)
    /// </summary>
    public async Task<SalesDashboardRespDto> GetSalesDashboardAsync(int sellerId, SalesDashboardReqDto request)
    {
        var (items, totalCount) = await _sellRepository.GetSalesDashboardAsync(
            sellerId,
            request.Status,
            request.Page,
            request.Size);

        // 포스터 이미지 Signed URL 배치 생성
        var posterKeys = items
            .Where(item => !string.IsNullOrEmpty(item.PosterImageUrl))
            .Select(item => item.PosterImageUrl!)
            .Where(url => !url.StartsWith("http://") && !url.StartsWith("https://"))
            .Distinct()
            .ToList();

        Dictionary<string, SignedUrlResult> posterSignedUrls = new();
        if (posterKeys.Any())
        {
            posterSignedUrls = await _fileUploadService.RefreshSignedUrlsBatchAsync(posterKeys);
        }

        var eventGroups = items.Select(item =>
        {
            string? signedPosterUrl = null;
            if (!string.IsNullOrEmpty(item.PosterImageUrl))
            {
                if (item.PosterImageUrl.StartsWith("http://") || item.PosterImageUrl.StartsWith("https://"))
                {
                    signedPosterUrl = item.PosterImageUrl;
                }
                else if (posterSignedUrls.TryGetValue(item.PosterImageUrl, out var urlResult))
                {
                    signedPosterUrl = urlResult.SignedUrl;
                }
            }

            return new EventGroupItemDto
            {
                EventId = item.EventId,
                EventTitle = item.EventTitle,
                PosterImageUrl = signedPosterUrl,
                VenueName = item.VenueName,
                EarliestEventDatetime = item.EarliestEventDatetime,
                TotalCount = item.TotalCount,
                OnSaleCount = item.OnSaleCount,
                CompletedCount = item.CompletedCount,
                SettlingCount = item.SettlingCount,
                RepresentativeSeatInfo = item.RepresentativeSeatInfo
            };
        }).ToList();

        return new SalesDashboardRespDto
        {
            EventGroups = eventGroups,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount,
            HasMore = (request.Page * request.Size) < totalCount
        };
    }

    /// <summary>
    /// 공연별 티켓 목록 조회
    /// </summary>
    public async Task<EventTicketListRespDto> GetEventTicketsAsync(int sellerId, int eventId, int page, int size)
    {
        var (items, totalCount) = await _sellRepository.GetEventTicketsAsync(sellerId, eventId, page, size);

        var thumbnailKeys = items
            .Where(item => !string.IsNullOrEmpty(item.ThumbnailPath))
            .Select(item => item.ThumbnailPath!)
            .Distinct()
            .ToList();

        Dictionary<string, SignedUrlResult> thumbnailSignedUrls = new();
        if (thumbnailKeys.Any())
        {
            thumbnailSignedUrls = await _fileUploadService.RefreshSignedUrlsBatchAsync(thumbnailKeys);
        }

        var firstItem = items.FirstOrDefault();
        var responseEventId = firstItem?.EventId ?? eventId;
        var responseEventTitle = firstItem?.EventTitle;
        if (string.IsNullOrWhiteSpace(responseEventTitle))
        {
            var eventInfo = await _sellRepository.GetEventByIdAsync(eventId);
            responseEventTitle = eventInfo?.Title ?? "공연 정보 없음";
        }

        var tickets = items.Select(item =>
        {
            var resolvedStatus = ResolveEventTicketDetailStatus(item);
            if (resolvedStatus is null)
            {
                return null;
            }

            string? thumbnailUrl = null;
            if (!string.IsNullOrEmpty(item.ThumbnailPath) &&
                thumbnailSignedUrls.TryGetValue(item.ThumbnailPath, out var thumbResult))
            {
                thumbnailUrl = thumbResult.SignedUrl;
            }

            var seatInfo = item.SeatInfo;
            if (string.IsNullOrWhiteSpace(seatInfo))
            {
                seatInfo = string.Join(" ", new[] { item.SeatGradeName, item.AreaName, item.Row }
                    .Where(s => !string.IsNullOrEmpty(s)));
            }

            if (string.IsNullOrWhiteSpace(seatInfo))
            {
                seatInfo = null;
            }

            return new EventTicketItemDto
            {
                TicketId = item.TicketId,
                SeatInfo = seatInfo,
                Quantity = item.Quantity,
                RemainingQuantity = item.RemainingQuantity,
                Price = item.Price,
                OriginalPrice = item.OriginalPrice,
                StatusCode = resolvedStatus.Value.StatusCode,
                StatusName = resolvedStatus.Value.StatusName,
                TransactionId = item.TransactionId,
                ThumbnailUrl = thumbnailUrl,
                CreatedAt = item.CreatedAt
            };
        })
        .Where(ticket => ticket is not null)
        .Select(ticket => ticket!)
        .ToList();

        return new EventTicketListRespDto
        {
            EventId = responseEventId,
            EventTitle = responseEventTitle,
            Tickets = tickets,
            Page = page,
            Size = size,
            TotalCount = totalCount,
            HasMore = (page * size) < totalCount
        };
    }

    private static (string StatusCode, string StatusName)? ResolveEventTicketDetailStatus(EventTicketReadModel item)
    {
        if (item.SettlementStatusCode == "on_hold")
        {
            return ("settlement_on_hold", "정산 보류");
        }

        if (item.SettlementStatusCode == "completed")
        {
            return ("settlement_completed", "정산 완료");
        }

        if (item.TransactionStatusCode is "cancelled" or "refunded")
        {
            return ("payment_cancelled", "결제 취소");
        }

        if (item.TransactionStatusCode is "confirmed" or "paid" or "completed")
        {
            return ("payment_completed", "결제 완료");
        }

        return null;
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

    /// <summary>
    /// 활성화된 티켓 특이사항 목록 조회
    /// </summary>
    public async Task<List<TicketFeatureRespDto>> GetTicketFeaturesAsync()
    {
        var features = await _sellRepository.GetActiveTicketFeaturesAsync();

        return features.Select(f => new TicketFeatureRespDto
        {
            Id = f.Id,
            Code = f.Code,
            NameKo = f.NameKo
        }).ToList();
    }

    /// <summary>
    /// 공연 좌석 정가 조회 (등급/위치/구역 기반)
    /// </summary>
    public async Task<int?> GetOriginalPriceAsync(GetOriginalPriceReqDto request)
    {
        // 현재는 GradeId 기준으로만 정가를 조회하지만, 필요 시 LocationId/AreaId 검증 로직 추가 가능
        var grade = await _sellRepository.GetSeatPriceAsync(request.EventId, request.GradeId);
        return grade?.OriginalPrice;
    }

    /// <summary>
    /// 활성화된 거래 방식 목록 조회
    /// </summary>
    public async Task<List<TradeMethodRespDto>> GetTradeMethodsAsync()
    {
        var tradeMethods = await _sellRepository.GetActiveTradeMethodsAsync();

        return tradeMethods.Select(tm => new TradeMethodRespDto
        {
            Id = tm.Id,
            Code = tm.Code,
            NameKo = tm.NameKo,
            NameEn = tm.NameEn,
            Description = tm.Description
        }).ToList();
    }
}
