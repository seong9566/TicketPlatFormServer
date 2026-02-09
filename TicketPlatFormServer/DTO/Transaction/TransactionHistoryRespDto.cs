using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DTO.Transaction;

/// <summary>
/// 거래 내역 목록 응답 DTO
/// </summary>
public class TransactionHistoryRespDto
{
    public List<TransactionHistoryItemDto> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }

    /// <summary>
    /// 전체 건수 (성능 최적화를 위해 첫 페이지에서만 조회하며, 이후 페이지에서는 null)
    /// </summary>
    public int? TotalCount { get; set; }
}

/// <summary>
/// 거래 내역 개별 항목 DTO
/// </summary>
public class TransactionHistoryItemDto
{
    public long TransactionId { get; set; }
    public int TicketId { get; set; }
    public string TicketTitle { get; set; } = null!;
    public string? TicketThumbnailUrl { get; set; }
    public DateTime EventDateTime { get; set; }
    public string? VenueName { get; set; }
    public string? SeatInfo { get; set; }
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
    public int TotalAmount { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public TransactionUserDto? Buyer { get; set; }
    public TransactionUserDto? Seller { get; set; }
    public long? RoomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

/// <summary>
/// 거래 상대방 정보 DTO (구매자/판매자)
/// </summary>
public class TransactionUserDto
{
    public long UserId { get; set; }
    public string Nickname { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
