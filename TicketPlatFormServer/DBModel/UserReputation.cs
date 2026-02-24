using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 평판 (리뷰) 테이블
/// </summary>
public partial class UserReputation
{
    public long Id { get; set; }

    /// <summary>
    /// 평가 대상 FK
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 평가자 FK
    /// </summary>
    public long ReviewerId { get; set; }

    /// <summary>
    /// 거래 FK
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 평가 유형 FK
    /// </summary>
    public long RatingTypeId { get; set; }

    /// <summary>
    /// 점수 (1-5)
    /// </summary>
    public int Score { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ReputationRatingType RatingType { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
