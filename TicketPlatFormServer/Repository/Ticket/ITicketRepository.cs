using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Ticket;

/// <summary>
/// 티켓 관련 Repository 인터페이스
/// </summary>
public interface ITicketRepository
{
    /// <summary>
    /// 이벤트의 티켓 목록 조회 (이벤트 상세 화면용 - 간단한 정보)
    /// </summary>
    /// <param name="eventId">이벤트 ID</param>
    /// <returns>티켓 목록</returns>
    Task<List<TicketListReadModel>> GetTicketsByEventId(int eventId);

    /// <summary>
    /// 티켓 상세 정보 조회 (티켓 상세 화면용 - 모든 정보)
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>티켓 상세 정보</returns>
    Task<TicketListReadModel?> GetTicketDetailById(int ticketId);
}
