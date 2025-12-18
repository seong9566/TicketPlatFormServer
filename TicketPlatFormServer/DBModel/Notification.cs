using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 알림 테이블
/// </summary>
public partial class Notification
{
    public long Id { get; set; }

    /// <summary>
    /// 수신자 FK
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 알림 유형 FK
    /// </summary>
    public long TypeId { get; set; }

    /// <summary>
    /// 알림 제목
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 알림 내용
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// 읽음 여부
    /// </summary>
    public bool? ReadFlag { get; set; }

    /// <summary>
    /// 읽은 시각
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 추가 데이터 (페이로드)
    /// </summary>
    public string? Data { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual NotificationType Type { get; set; } = null!;
}
