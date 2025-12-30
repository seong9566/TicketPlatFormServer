using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Services.Ticket;

/// <summary>
/// 티켓 관련 Service 인터페이스
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// 티켓 상세 정보 조회
    /// </summary>
    /// <param name="ticketId">티켓 ID</param>
    /// <returns>티켓 상세 정보</returns>
    Task<TicketListRespDto> GetTicketDetailById(int ticketId);
}
