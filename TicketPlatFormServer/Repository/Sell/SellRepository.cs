using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Sell;

/// <summary>
/// 티켓 판매 Repository 구현체
/// </summary>
public class SellRepository(TicketContext context, IDbConnection dapper) : ISellRepository
{
    private readonly TicketContext _context = context;
    private readonly IDbConnection _dapper = dapper;

    /// <summary>
    /// ExecutionStrategy 생성 (MySQL retry 지원)
    /// </summary>
    public Task<IExecutionStrategy> CreateExecutionStrategyAsync()
    {
        return Task.FromResult(_context.Database.CreateExecutionStrategy());
    }

    /// <summary>
    /// 트랜잭션 시작
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// 활성화된 카테고리 목록 조회
    /// </summary>
    public async Task<List<TicketCategory>> GetActiveCategoriesAsync()
    {
        return await _context.TicketCategories
            .Where(c => c.IsActive == true)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 카테고리별 공연 목록 조회 (페이징)
    /// </summary>
    public async Task<(List<Event> Events, int TotalCount)> GetEventsByCategoryAsync(
        int categoryId,
        string? keyword,
        int page,
        int size)
    {
        var query = _context.Events
            .Where(e => e.CategoryId == categoryId && e.IsActive == true);

        // 키워드 검색
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(e =>
                e.Title.Contains(keyword) ||
                (e.VenueName != null && e.VenueName.Contains(keyword)));
        }

        // 전체 개수
        var totalCount = await query.CountAsync();

        // 페이징
        var events = await query
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.StartAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (events, totalCount);
    }

    /// <summary>
    /// 특정 공연의 일정 목록 조회
    /// </summary>
    public async Task<List<EventSchedule>> GetEventSchedulesAsync(int eventId)
    {
        return await _context.EventSchedules
            .Where(es => es.EventId == eventId && es.IsActive == true)
            .OrderBy(es => es.ScheduleDate)
            .ThenBy(es => es.ScheduleTime)
            .ToListAsync();
    }

    /// <summary>
    /// 특정 공연의 좌석 등급 옵션 조회 (정가 포함)
    /// Dapper로 최적화 - 단일 SQL로 JOIN 처리
    /// </summary>
    public async Task<List<(EventSeatGrade Grade, int? OriginalPrice)>> GetSeatGradesAsync(int eventId)
    {
        // Dapper로 좌석 등급(통합) 조회
        var readModels = await _dapper.QueryAsync<SeatGradeReadModel>(
            SellQueries.GetSeatGradesWithPrices,
            new { EventId = eventId }
        );

        // ReadModel → (EventSeatGrade, int?) 튜플 변환
        // 이제 EventSeatGrade 자체에 명칭과 정가가 포함됨
        var result = readModels.Select(rm => (
            new EventSeatGrade
            {
                Id = rm.GradeId,
                EventId = rm.EventId,
                SeatGradeId = rm.SeatGradeId,
                Code = rm.Code,
                NameKo = rm.NameKo,
                NameEn = rm.NameEn,
                OriginalPrice = rm.OriginalPrice,
                SortOrder = rm.SortOrder,
                IsActive = true
            },
            rm.OriginalPrice
        )).ToList();

        return result;
    }

    /// <summary>
    /// 특정 공연의 좌석 위치 옵션 조회
    /// </summary>
    public async Task<List<EventSeatLocation>> GetSeatLocationsAsync(int eventId)
    {
        // 해당 공연 전용 좌석 위치
        return await _context.EventSeatLocations
            .Where(sl => sl.EventId == eventId && sl.IsActive == true)
            .OrderBy(sl => sl.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 특정 공연의 좌석 구역 옵션 조회
    /// </summary>
    public async Task<List<EventSeatArea>> GetSeatAreasAsync(int eventId)
    {
        return await _context.EventSeatAreas
            .Where(sa => sa.EventId == eventId && sa.IsActive == true)
            .OrderBy(sa => sa.SortOrder)
            .ToListAsync();
    }


    /// <summary>
    /// 공연 조회
    /// </summary>
    public async Task<Event?> GetEventByIdAsync(int eventId)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    /// <summary>
    /// 일정 조회
    /// </summary>
    public async Task<EventSchedule?> GetScheduleByIdAsync(string scheduleId)
    {
        return await _context.EventSchedules
            .FirstOrDefaultAsync(es => es.Id == scheduleId);
    }

    /// <summary>
    /// 티켓 생성
    /// </summary>
    public async Task<int> CreateTicketAsync(DBModel.Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket.Id;
    }

    /// <summary>
    /// 티켓 이미지 생성
    /// </summary>
    public async Task CreateTicketImagesAsync(List<TicketImage> images)
    {
        if (images.Any())
        {
            _context.TicketImages.AddRange(images);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 티켓 이미지 목록 조회 (배치 - N+1 방지)
    /// </summary>
    public async Task<Dictionary<int, List<TicketImage>>> GetTicketImagesByTicketIdsAsync(List<int> ticketIds)
    {
        if (!ticketIds.Any())
            return new Dictionary<int, List<TicketImage>>();

        var images = await _context.TicketImages
            .Where(ti => ticketIds.Contains((int)ti.TicketId))
            .OrderBy(ti => ti.Id) // 첫 번째 이미지 = 썸네일
            .ToListAsync();

        return images
            .GroupBy(ti => (int)ti.TicketId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// 특정 티켓의 이미지 목록 조회
    /// </summary>
    public async Task<List<TicketImage>> GetTicketImagesByTicketIdAsync(int ticketId)
    {
        return await _context.TicketImages
            .Where(ti => ti.TicketId == ticketId)
            .OrderBy(ti => ti.Id)
            .ToListAsync();
    }

    /// <summary>
    /// 사용자의 판매 티켓 목록 조회 (페이징)
    /// Dapper로 최적화 - 복잡한 JOIN 단일 쿼리 처리
    /// </summary>
    public async Task<(List<DBModel.Ticket> Tickets, int TotalCount)> GetMyTicketsAsync(
        int sellerId,
        string? status,
        int page,
        int size)
    {
        var offset = (page - 1) * size;

        // 1. 총 개수 조회
        var totalCount = await _dapper.ExecuteScalarAsync<int>(
            SellQueries.GetMyTicketsCount,
            new { SellerId = sellerId, Status = status }
        );

        // 2. 티켓 목록 조회 (Dapper)
        var readModels = await _dapper.QueryAsync<MyTicketReadModel>(
            SellQueries.GetMyTickets,
            new { SellerId = sellerId, Status = status, Limit = size, Offset = offset }
        );

        // 3. ReadModel → DBModel.Ticket 변환
        var tickets = readModels.Select(rm => new DBModel.Ticket
        {
            Id = rm.TicketId,
            EventId = rm.EventId,
            SeatGradeId = rm.SeatGradeId,
            Price = rm.Price,
            Quantity = rm.Quantity,
            RemainingQuantity = rm.RemainingQuantity,
            StatusId = rm.StatusId,
            CreatedAt = rm.CreatedAt,
            // Navigation Properties (Dapper는 별도 매핑)
            Event = new Event { Id = rm.EventId, Title = rm.EventTitle },
            SeatGrade = rm.SeatGradeId.HasValue
                ? new EventSeatGrade { Id = rm.SeatGradeId.Value, NameKo = rm.SeatGradeName! }
                : null,
            Area = !string.IsNullOrEmpty(rm.AreaName)
                ? new EventSeatArea { AreaName = rm.AreaName }
                : null,
            Status = new TicketStatus
            {
                Id = rm.StatusId,
                Code = rm.StatusCode,
                NameKo = rm.StatusName
            }
        }).ToList();

        return (tickets, totalCount);
    }

    /// <summary>
    /// 티켓 조회
    /// </summary>
    public async Task<DBModel.Ticket?> GetTicketByIdAsync(int ticketId)
    {
        return await _context.Tickets
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.DeletedAt == null);
    }

    /// <summary>
    /// 티켓 상태 업데이트
    /// </summary>
    public async Task UpdateTicketStatusAsync(int ticketId, int statusId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.StatusId = statusId;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 상태 코드로 티켓 상태 ID 조회
    /// </summary>
    public async Task<int?> GetTicketStatusIdByCodeAsync(string code)
    {
        var status = await _context.TicketStatuses
            .FirstOrDefaultAsync(s => s.Code == code);
        return status?.Id;
    }

    /// <summary>
    /// 활성화된 티켓 특이사항 목록 조회
    /// </summary>
    public async Task<List<TicketFeature>> GetActiveTicketFeaturesAsync()
    {
        return await _context.TicketFeatures
            .OrderBy(f => f.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// 티켓-특이사항 연결 생성
    /// </summary>
    public async Task CreateTicketFeaturesAsync(int ticketId, List<int> featureIds)
    {
        // 이제 티켓 엔티티의 feature_ids 컬럼에 직접 저장하므로 이 메서드는 하위 호환성을 위해 유지하거나 
        // 필요한 경우 해당 컬럼을 업데이트하는 용도로 쓰일 수 있음.
        // 현재는 SellService에서 티켓 생성 시 직접 넣어주고 있으므로 여기서는 아무것도 하지 않음.
        await Task.CompletedTask;
    }

    /// <summary>
    /// 공연별 좌석 정가 조회 (통합된 EventSeatGrade 사용)
    /// </summary>
    public async Task<EventSeatGrade?> GetSeatPriceAsync(int eventId, int seatGradeId)
    {
        return await _context.EventSeatGrades
            .FirstOrDefaultAsync(esg => esg.EventId == eventId
                                     && esg.SeatGradeId == seatGradeId
                                     && esg.IsActive == true);
    }
}

