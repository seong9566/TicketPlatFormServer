using TicketPlatFormServer.DTO.Sell;

namespace TicketPlatFormServer.Services.Sell;

/// <summary>
/// 티켓 판매 Service 인터페이스
/// </summary>
public interface ISellService
{
    /// <summary>
    /// 판매 가능한 카테고리 목록 조회
    /// </summary>
    Task<List<CategoryRespDto>> GetCategoriesAsync();

    /// <summary>
    /// 카테고리별 공연 목록 조회 (페이징)
    /// </summary>
    Task<SellEventListRespDto> GetEventsAsync(SellEventListReqDto request);

    /// <summary>
    /// 특정 공연의 일정 목록 조회
    /// </summary>
    Task<EventScheduleRespDto> GetEventSchedulesAsync(int eventId);

    /// <summary>
    /// 특정 공연의 좌석 옵션 조회
    /// </summary>
    Task<SeatOptionRespDto> GetSeatOptionsAsync(int eventId);

    /// <summary>
    /// 티켓 판매 등록
    /// </summary>
    Task<CreateSellTicketRespDto> CreateTicketAsync(int userId, CreateSellTicketReqDto request);

    /// <summary>
    /// 내 판매 티켓 목록 조회
    /// </summary>
    Task<MyTicketListRespDto> GetMyTicketsAsync(int userId, MyTicketListReqDto request);

    /// <summary>
    /// 티켓 판매 취소
    /// </summary>
    Task<CancelSellTicketRespDto> CancelTicketAsync(int userId, int ticketId);

    /// <summary>
    /// 티켓 이미지 URL 재발급
    /// </summary>
    Task<RefreshTicketImageUrlRespDto> RefreshTicketImageUrlsAsync(int ticketId, int userId);
}
