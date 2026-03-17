namespace TicketPlatFormServer.DTO.Chat;

/// <summary>
/// 거래 요청 DTO (판매자가 구매자에게 거래 요청 시 사용)
/// </summary>
public class RequestPaymentReqDto
{
    /// <summary>채팅방 ID</summary>
    public long RoomId { get; set; }

    /// <summary>구매 수량</summary>
    public int Quantity { get; set; }
}
