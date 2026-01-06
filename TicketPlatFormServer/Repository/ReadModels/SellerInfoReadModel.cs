namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 판매자 정보 ReadModel (Repository 반환용)
/// </summary>
public class SellerInfoReadModel
{
    public int UserId { get; set; }

    /// <summary>
    /// 닉네임
    /// </summary>
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// 프로필 이미지 URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// 매너 온도
    /// </summary>
    public float? MannerTemperature { get; set; }

    /// <summary>
    /// 총 거래 횟수
    /// </summary>
    public int TotalTradeCount { get; set; }

    /// <summary>
    /// 응답률 (0-100, 판매자가 채팅에 응답한 비율)
    /// </summary>
    public float? ResponseRate { get; set; }

    /// <summary>
    /// 안심결제 가능 여부 (본인인증, 휴대폰인증, 계좌인증 모두 완료)
    /// </summary>
    public bool IsSecurePayment { get; set; }
}
