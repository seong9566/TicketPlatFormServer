namespace TicketPlatFormServer.DTO;

/// <summary>
/// 아이디 찾기 요청 DTO
/// </summary>
public class FindIdReqDto
{
    /// <summary>
    /// 전화번호
    /// </summary>
    public string PhoneNumber { get; set; } = null!;
}
