using System.Data;
using Dapper;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Ticket;

/// <summary>
/// 티켓 관련 Repository 구현체
/// </summary>
public class TicketRepository(
    IDbConnection dapper) : ITicketRepository
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
            tickets.Add(new TicketListReadModel
            {
                TicketId = row.TicketId,
                SeatGradeId = row.SeatGradeId,
                SeatGradeName = row.SeatGradeName,
                Area = row.Area,
                Row = row.Row,
                Price = row.Price,
                OriginalPrice = row.OriginalPrice,
                IsConsecutive = row.IsConsecutive,
                TradeMethodId = row.TradeMethodId,
                TradeMethodName = row.TradeMethodName,
                HasTicket = row.HasTicket,
                Description = row.Description,
                CreatedAt = row.CreatedAt,
                Quantity = row.Quantity,
                IsSingleTicket = row.Quantity == 1,
                RemainingQuantity = row.RemainingQuantity,
                // 목록에서는 이미지 생략
                TicketImages = new List<string>(),
                Seller = new SellerInfoReadModel
                {
                    UserId = row.UserId,
                    Nickname = row.Nickname,
                    ProfileImageUrl = row.ProfileImageUrl,
                    MannerTemperature = row.MannerTemperature != null ? (float?)Convert.ToDouble(row.MannerTemperature) : null,
                    // 목록에서는 상세 정보 생략
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

        // 티켓 이미지 조회
        var ticketImages = await dapper.QueryAsync<string>(
            TicketQueries.GetTicketImages,
            new { TicketId = ticketId }
        );

        return new TicketListReadModel
        {
            TicketId = ticketRow.TicketId,
            SeatGradeId = ticketRow.SeatGradeId,
            SeatGradeName = ticketRow.SeatGradeName,
            Area = ticketRow.Area,
            Row = ticketRow.Row,
            Price = ticketRow.Price,
            OriginalPrice = ticketRow.OriginalPrice,
            IsConsecutive = ticketRow.IsConsecutive,
            TradeMethodId = ticketRow.TradeMethodId,
            TradeMethodName = ticketRow.TradeMethodName,
            HasTicket = ticketRow.HasTicket,
            Description = ticketRow.Description,
            CreatedAt = ticketRow.CreatedAt,
            Quantity = ticketRow.Quantity,
            IsSingleTicket = ticketRow.Quantity == 1,
            RemainingQuantity = ticketRow.RemainingQuantity,
            TicketImages = ticketImages.ToList(),
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
    /// 티켓에 연결된 특이사항 목록 조회 (비정규화 컬럼 feature_ids 기반)
    /// </summary>
    public async Task<List<TicketFeatureReadModel>> GetTicketFeaturesAsync(int ticketId)
    {
        // 1. 티켓에서 feature_ids 문자열 조회
        var featureIdsStr = await dapper.QueryFirstOrDefaultAsync<string>(
            "SELECT feature_ids FROM tickets WHERE id = @TicketId",
            new { TicketId = ticketId }
        );

        if (string.IsNullOrWhiteSpace(featureIdsStr))
        {
            return new List<TicketFeatureReadModel>();
        }

        // 2. ID 리스트로 변환
        var ids = featureIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(int.Parse)
                               .ToList();

        if (!ids.Any())
        {
            return new List<TicketFeatureReadModel>();
        }

        // 3. 실제 특징 정보(명칭 등) 조회
        var features = await dapper.QueryAsync<TicketFeatureReadModel>(
            TicketQueries.GetTicketFeaturesByIds,
            new { Ids = ids }
        );

        return features.ToList();
    }
}

