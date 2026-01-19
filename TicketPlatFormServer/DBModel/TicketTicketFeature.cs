using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓-특이사항 연결 테이블
/// </summary>
public partial class TicketTicketFeature
{
    public int Id { get; set; }

    /// <summary>
    /// 티켓 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 특이사항 ID
    /// </summary>
    public int FeatureId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
