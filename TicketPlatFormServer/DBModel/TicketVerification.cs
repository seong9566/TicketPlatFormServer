using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 검증 테이블
/// </summary>
public partial class TicketVerification
{
    public long Id { get; set; }

    /// <summary>
    /// 거래 FK
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// 검증 방법 FK
    /// </summary>
    public long MethodId { get; set; }

    /// <summary>
    /// OCR/QR 원본 데이터
    /// </summary>
    public string? RawData { get; set; }

    /// <summary>
    /// 검증 결과
    /// </summary>
    public bool? VerificationResult { get; set; }

    /// <summary>
    /// 검증자 FK (수동 검증 시)
    /// </summary>
    public long? VerifiedBy { get; set; }

    /// <summary>
    /// OCR 신뢰도
    /// </summary>
    public float? OcrConfidence { get; set; }

    /// <summary>
    /// QR코드 해시
    /// </summary>
    public string? QrCodeHash { get; set; }

    /// <summary>
    /// 티켓 번호
    /// </summary>
    public string? TicketNumber { get; set; }

    /// <summary>
    /// 검증 시각
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    public virtual TicketVerificationMethod Method { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
