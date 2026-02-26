namespace TicketPlatFormServer.DTO;

/// <summary>
/// 아이디 찾기 응답 DTO
/// </summary>
public class FindIdResDto
{
    /// <summary>
    /// 마스킹된 이메일
    /// </summary>
    public string MaskedEmail { get; set; } = null!;
}
