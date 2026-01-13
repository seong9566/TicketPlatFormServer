using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Sell;

/// <summary>
/// 티켓 판매 Repository 인터페이스
/// </summary>
public interface ISellRepository
{
    /// <summary>
    /// 활성화된 카테고리 목록 조회
    /// </summary>
    Task<List<TicketCategory>> GetActiveCategoriesAsync();

    /// <summary>
    /// 카테고리별 공연 목록 조회 (페이징)
    /// </summary>
    Task<(List<Event> Events, int TotalCount)> GetEventsByCategoryAsync(
        int categoryId,
        string? keyword,
        int page,
        int size);

    /// <summary>
    /// 특정 공연의 일정 목록 조회
    /// </summary>
    Task<List<EventSchedule>> GetEventSchedulesAsync(int eventId);

    /// <summary>
    /// 특정 공연의 좌석 위치 옵션 조회
    /// </summary>
    Task<List<SeatLocation>> GetSeatLocationsAsync(int eventId);

    /// <summary>
    /// 공연 조회
    /// </summary>
    Task<Event?> GetEventByIdAsync(int eventId);

    /// <summary>
    /// 일정 조회
    /// </summary>
    Task<EventSchedule?> GetScheduleByIdAsync(string scheduleId);

    /// <summary>
    /// 티켓 생성
    /// </summary>
    Task<int> CreateTicketAsync(DBModel.Ticket ticket);

    /// <summary>
    /// 티켓 이미지 생성
    /// </summary>
    Task CreateTicketImagesAsync(List<TicketImage> images);

    /// <summary>
    /// 사용자의 판매 티켓 목록 조회 (페이징)
    /// </summary>
    Task<(List<DBModel.Ticket> Tickets, int TotalCount)> GetMyTicketsAsync(
        int sellerId,
        string? status,
        int page,
        int size);

    /// <summary>
    /// 티켓 조회
    /// </summary>
    Task<DBModel.Ticket?> GetTicketByIdAsync(int ticketId);

    /// <summary>
    /// 티켓 상태 업데이트
    /// </summary>
    Task UpdateTicketStatusAsync(int ticketId, int statusId);

    /// <summary>
    /// 상태 코드로 티켓 상태 ID 조회
    /// </summary>
    Task<int?> GetTicketStatusIdByCodeAsync(string code);
}
