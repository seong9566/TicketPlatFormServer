using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class TicketVerification
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public string Method { get; set; } = null!;

    public string? RawData { get; set; }

    public bool? VerificationResult { get; set; }

    public long? VerifiedBy { get; set; }

    public float? OcrConfidence { get; set; }

    public string? QrCodeHash { get; set; }

    public string? TicketNumber { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }
}
