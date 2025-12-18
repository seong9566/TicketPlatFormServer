using TicketPlatFormServer.DTO.Home;

namespace TicketPlatFormServer.Services.Home;

/// <summary>
/// 홈 화면 Service 인터페이스
/// </summary>
public interface IHomeService
{
    /// <summary>
    /// 홈 화면 데이터 조회
    /// </summary>
    Task<HomeRespDto> GetHomeData(int? userId);
}

