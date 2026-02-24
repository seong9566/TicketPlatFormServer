using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 정산 정보 테이블
/// </summary>
public partial class Settlement
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    /// <summary>
    /// 판매자 FK
    /// </summary>
    public long SellerId { get; set; }

    /// <summary>
    /// 총 금액
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 수수료
    /// </summary>
    public int Fee { get; set; }

    /// <summary>
    /// 순 정산 금액
    /// </summary>
    public int NetAmount { get; set; }

    /// <summary>
    /// 정산 계좌 FK
    /// </summary>
    public long? BankAccountId { get; set; }

    /// <summary>
    /// 상태 FK
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// 정산 예정 일시
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// 정산 완료 시각
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 실패 사유
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 재시도 횟수
    /// </summary>
    public int? RetryCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual BankAccount? BankAccount { get; set; }

    public virtual SettlementStatus Status { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
