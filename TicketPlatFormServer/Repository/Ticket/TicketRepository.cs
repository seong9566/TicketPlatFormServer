using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Ticket;

/// <summary>
/// 티켓 관련 Repository 구현체 (Primary Constructor + Static Class 패턴 + 안전한 JSON 로깅)
/// </summary>
public class TicketRepository(
    TicketContext db,
    IDbConnection dapper,
    ILogger<TicketRepository> logger) : ITicketRepository
{

    public async Task<List<TicketListReadModel>> GetTicketsByEventId(int eventId)
    {
        // 이벤트의 티켓 목록 조회 (간단한 정보만)
        var ticketRows = await dapper.QueryAsync<dynamic>(
            TicketQueries.GetTicketsByEventId,
            new { EventId = eventId }
        );

        var tickets = new List<TicketListReadModel>();

        foreach (var row in ticketRows)
        {
            // 좌석 타입 추출
            string? seatType = ExtractSeatType(row.TicketTitle, row.SeatFeatures);

            // SeatFeatures JSON 파싱
            List<string> seatFeatures = new();
            if (row.SeatFeatures != null)
            {
                try
                {
                    var features = JsonSerializer.Deserialize<List<string>>(row.SeatFeatures.ToString() ?? "[]");
                    if (features != null)
                    {
                        seatFeatures = features;
                    }
                }
                catch (JsonException ex)
                {
                    // JSON 파싱 실패 시 로깅 (보안: 민감한 데이터 노출 방지)
                    logger.LogWarning(ex,
                        "[TicketRepository] JSON 파싱 실패 | TicketId: {TicketId}",
                        (int)row.TicketId);
                }
            }

            tickets.Add(new TicketListReadModel
            {
                TicketId = row.TicketId,
                TicketTitle = row.TicketTitle,
                SeatInfo = row.SeatInfo,
                SeatType = seatType,
                Price = row.Price,
                OriginalPrice = row.OriginalPrice,
                SeatFeatures = seatFeatures,
                CreatedAt = row.CreatedAt,
                Quantity = row.Quantity,
                IsSingleTicket = row.Quantity == 1,
                RemainingQuantity = row.RemainingQuantity,
                // 이벤트 목록에서는 상세 정보 제외
                Description = row.Description,
                TicketImages = new List<string>(),
                Seller = new SellerInfoReadModel
                {
                    UserId = row.UserId,
                    Nickname = row.Nickname,
                    ProfileImageUrl = row.ProfileImageUrl,
                    MannerTemperature = row.MannerTemperature != null ? (float?)Convert.ToDouble(row.MannerTemperature) : null,
                    // 이벤트 목록에서는 상세 정보 제외
                    TotalTradeCount = 0,
                    ResponseRate = null,
                    IsSecurePayment = false
                }
            });
        }

        return tickets;
    }

    public async Task<TicketListReadModel?> GetTicketDetailById(int ticketId)
    {
        // 티켓 상세 정보 조회
        var ticketRow = await dapper.QueryFirstOrDefaultAsync<dynamic>(
            TicketQueries.GetTicketDetailById,
            new { TicketId = ticketId }
        );

        if (ticketRow == null)
        {
            return null;
        }

        // 좌석 타입 추출
        string? seatType = ExtractSeatType(ticketRow.TicketTitle, ticketRow.SeatFeatures);

        // SeatFeatures JSON 파싱
        List<string> seatFeatures = new();
        if (ticketRow.SeatFeatures != null)
        {
            try
            {
                var features = JsonSerializer.Deserialize<List<string>>(ticketRow.SeatFeatures.ToString() ?? "[]");
                if (features != null)
                {
                    seatFeatures = features;
                }
            }
            catch (JsonException ex)
            {
                // JSON 파싱 실패 시 로깅 (보안: 민감한 데이터 노출 방지)
                logger.LogWarning(ex,
                    "[TicketRepository.GetTicketDetailById] JSON 파싱 실패 | TicketId: {TicketId}",
                    (int)ticketRow.TicketId);
            }
        }

        // 티켓 이미지 조회
        var ticketImages = await dapper.QueryAsync<string>(
            TicketQueries.GetTicketImages,
            new { TicketId = ticketId }
        );

        return new TicketListReadModel
        {
            TicketId = ticketRow.TicketId,
            TicketTitle = ticketRow.TicketTitle,
            SeatInfo = ticketRow.SeatInfo,
            SeatType = seatType,
            Price = ticketRow.Price,
            OriginalPrice = ticketRow.OriginalPrice,
            SeatFeatures = seatFeatures,
            Description = ticketRow.Description,
            CreatedAt = ticketRow.CreatedAt,
            Quantity = ticketRow.Quantity,
            IsSingleTicket = ticketRow.Quantity == 1,
            TicketImages = ticketImages.ToList(),
            RemainingQuantity = ticketRow.RemainingQuantity,
            Seller = new SellerInfoReadModel
            {
                UserId = ticketRow.UserId,
                Nickname = ticketRow.Nickname,
                ProfileImageUrl = ticketRow.ProfileImageUrl,
                MannerTemperature = ticketRow.MannerTemperature != null ? (float?)Convert.ToDouble(ticketRow.MannerTemperature) : null,
                TotalTradeCount = ticketRow.TotalTradeCount ?? 0,
                ResponseRate = ticketRow.ResponseRate != null ? (float?)Convert.ToDouble(ticketRow.ResponseRate) : null,
                IsSecurePayment = ticketRow.IsSecurePayment == 1
            }
        };
    }

    /// <summary>
    /// 티켓 제목 또는 seat_features에서 좌석 타입 추출
    /// </summary>
    private string? ExtractSeatType(string? ticketTitle, object? seatFeatures)
    {
        if (!string.IsNullOrEmpty(ticketTitle))
        {
            // 티켓 제목에서 좌석 타입 추출 (예: "위키드 VIP석" -> "VIP석")
            if (ticketTitle.Contains("VIP"))
                return "VIP석";
            if (ticketTitle.Contains("R석") || ticketTitle.Contains(" R "))
                return "R석";
            if (ticketTitle.Contains("S석") || ticketTitle.Contains(" S "))
                return "S석";
            if (ticketTitle.Contains("A석") || ticketTitle.Contains(" A "))
                return "A석";
        }

        // seat_features JSON에서 추출
        if (seatFeatures != null)
        {
            try
            {
                var features = JsonSerializer.Deserialize<List<string>>(seatFeatures.ToString() ?? "[]");
                if (features != null)
                {
                    // VIP, R, S, A 등 좌석 타입 키워드 찾기
                    var seatTypeKeywords = new[] { "VIP", "R", "S", "A" };
                    foreach (var keyword in seatTypeKeywords)
                    {
                        if (features.Any(f => f.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                        {
                            return keyword == "VIP" ? "VIP석" : $"{keyword}석";
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                // JSON 파싱 실패 시 로깅 (보안: 민감한 데이터 노출 방지)
                logger.LogWarning(ex, "[TicketRepository.ExtractSeatType] JSON 파싱 실패");
            }
        }

        return null;
    }
}
