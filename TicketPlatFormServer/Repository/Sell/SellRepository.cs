using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Sell;

/// <summary>
/// 티켓 판매 Repository 구현체
/// </summary>
public class SellRepository(TicketContext context) : ISellRepository
{
    private readonly TicketContext _context = context;

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
    /// </summary>
    public async Task<(List<DBModel.Ticket> Tickets, int TotalCount)> GetMyTicketsAsync(
        int sellerId,
        string? status,
        int page,
        int size)
    {
        var query = _context.Tickets
            .Include(t => t.Status)
            .Where(t => t.SellerId == sellerId && t.DeletedAt == null);

        // 상태 필터
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status.Code == status);
        }

        // 전체 개수
        var totalCount = await query.CountAsync();

        // 페이징
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

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
}
