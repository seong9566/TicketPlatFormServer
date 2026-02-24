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
    /// 공연 좌석 정가 조회 (등급/위치/구역 기반)
    /// </summary>
    Task<int?> GetOriginalPriceAsync(GetOriginalPriceReqDto request);

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

    /// <summary>
    /// 활성화된 티켓 특이사항 목록 조회
    /// </summary>
    Task<List<TicketFeatureRespDto>> GetTicketFeaturesAsync();

    /// <summary>
    /// 활성화된 거래 방식 목록 조회
    /// </summary>
    Task<List<TradeMethodRespDto>> GetTradeMethodsAsync();

    /// <summary>
    /// 판매 대시보드 조회 (공연별 그룹화, 페이징)
    /// </summary>
    Task<SalesDashboardRespDto> GetSalesDashboardAsync(int sellerId, SalesDashboardReqDto request);

    /// <summary>
    /// 공연별 티켓 목록 조회 (페이징)
    /// </summary>
    Task<EventTicketListRespDto> GetEventTicketsAsync(int sellerId, int eventId, int page, int size);
}

