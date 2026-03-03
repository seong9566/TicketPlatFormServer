using TicketPlatFormServer.DBModel;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Sell;

/// <summary>
/// 티켓 판매 Repository 인터페이스
/// </summary>
public interface ISellRepository
{
    /// <summary>
    /// ExecutionStrategy 생성 (MySQL retry 지원)
    /// </summary>
    IExecutionStrategy CreateExecutionStrategy();

    /// <summary>
    /// 트랜잭션 시작
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync();

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
    /// 특정 공연의 좌석 등급 옵션 조회 (정가 포함)
    /// </summary>
    Task<List<(EventSeatGrade Grade, int? OriginalPrice)>> GetSeatGradesAsync(int eventId);

    /// <summary>
    /// 특정 공연의 좌석 위치 옵션 조회
    /// </summary>
    Task<List<EventSeatLocation>> GetSeatLocationsAsync(int eventId);

    /// <summary>
    /// 특정 공연의 좌석 구역 옵션 조회
    /// </summary>
    Task<List<EventSeatArea>> GetSeatAreasAsync(int eventId);


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
    /// 티켓 이미지 목록 조회 (배치 - N+1 방지)
    /// </summary>
    Task<Dictionary<int, List<TicketImage>>> GetTicketImagesByTicketIdsAsync(List<int> ticketIds);

    /// <summary>
    /// 특정 티켓의 이미지 목록 조회
    /// </summary>
    Task<List<TicketImage>> GetTicketImagesByTicketIdAsync(long ticketId);

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

    /// <summary>
    /// 활성화된 티켓 특이사항 목록 조회
    /// </summary>
    Task<List<TicketFeature>> GetActiveTicketFeaturesAsync();

    /// <summary>
    /// 티켓-특이사항 연결 생성
    /// </summary>
    Task CreateTicketFeaturesAsync(int ticketId, List<int> featureIds);

    /// <summary>
    /// 공연별 좌석 정가 조회 (통합된 EventSeatGrade 사용)
    /// </summary>
    Task<EventSeatGrade?> GetSeatPriceAsync(int eventId, int seatGradeId);

    /// <summary>
    /// 판매 대시보드 조회 (공연별 그룹화)
    /// </summary>
    Task<(List<SalesDashboardReadModel> Items, int TotalCount)> GetSalesDashboardAsync(
        int sellerId,
        string? statusFilter,
        int page,
        int size);

    /// <summary>
    /// 공연별 티켓 목록 조회
    /// </summary>
    Task<(List<EventTicketReadModel> Items, int TotalCount)> GetEventTicketsAsync(
        int sellerId,
        int eventId,
        int page,
        int size);

    /// <summary>
    /// 활성화된 거래 방식 목록 조회
    /// </summary>
    Task<List<TradeMethod>> GetActiveTradeMethodsAsync();
}
