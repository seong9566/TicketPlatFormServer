using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓-특징 매핑 테이블 (Many-to-Many)
/// </summary>
public partial class TicketTicketFeature
{
    public int Id { get; set; }

    /// <summary>
    /// 티켓 FK
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 특징 FK
    /// </summary>
    public int FeatureId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual TicketFeature TicketFeature { get; set; } = null!;
}
