using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 프로필 테이블
/// </summary>
public partial class UserProfile
{
    public long UserId { get; set; }

    /// <summary>
    /// 닉네임
    /// </summary>
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// 프로필 이미지 URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// 자기소개
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// 구매자 평점
    /// </summary>
    public float? BuyerRating { get; set; }

    /// <summary>
    /// 구매 거래 횟수
    /// </summary>
    public int? BuyerTradeCount { get; set; }

    public virtual User User { get; set; } = null!;
}
