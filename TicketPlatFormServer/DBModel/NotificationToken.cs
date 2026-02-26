using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 알림 디바이스 토큰 테이블
/// </summary>
public partial class NotificationToken
{
    public long Id { get; set; }

    /// <summary>
    /// 사용자 FK
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// FCM/APNs 토큰
    /// </summary>
    public string DeviceToken { get; set; } = null!;

    /// <summary>
    /// 플랫폼 FK
    /// </summary>
    public long PlatformId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual NotificationPlatform Platform { get; set; } = null!;
}
