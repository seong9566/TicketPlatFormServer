using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 프로필 테이블
/// </summary>
public partial class UserProfile
{
    public int UserId { get; set; }

    public string? Nickname { get; set; }

    /// <summary>
    /// 프로필 이미지 URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// 자기소개
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// 매너 온도 (36.5~99.9)
    /// </summary>
    public float? MannerTemperature { get; set; }

    /// <summary>
    /// 총 거래 횟수
    /// </summary>
    public int? TotalTradeCount { get; set; }
}
