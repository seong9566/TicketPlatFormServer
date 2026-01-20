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
                TicketId = (int)row.TicketId,
                // 좌석 등급 정보 (확장)
                SeatGradeId = row.SeatGradeId != null ? (int?)Convert.ToInt32(row.SeatGradeId) : null,
                SeatGradeCode = row.SeatGradeCode,
                SeatGradeName = row.SeatGradeName,
                SeatGradeNameEn = row.SeatGradeNameEn,
                SeatGradeSortOrder = row.SeatGradeSortOrder != null ? (int?)Convert.ToInt32(row.SeatGradeSortOrder) : null,
                // 구역 정보 (확장)
                AreaId = row.AreaId != null ? (int?)Convert.ToInt32(row.AreaId) : null,
                Area = row.Area,
                AreaSortOrder = row.AreaSortOrder != null ? (int?)Convert.ToInt32(row.AreaSortOrder) : null,
                // 위치 정보 (NEW)
                LocationId = row.LocationId != null ? (int?)Convert.ToInt32(row.LocationId) : null,
                LocationName = row.LocationName,
                LocationSortOrder = row.LocationSortOrder != null ? (int?)Convert.ToInt32(row.LocationSortOrder) : null,
                // 기존 필드
                Row = row.Row,
                Price = (int)row.Price,
                OriginalPrice = (int)row.OriginalPrice,
                IsConsecutive = row.IsConsecutive,
                TradeMethodId = row.TradeMethodId != null ? (int?)Convert.ToInt32(row.TradeMethodId) : null,
                TradeMethodName = row.TradeMethodName,
                HasTicket = row.HasTicket,
                Description = row.Description,
                CreatedAt = row.CreatedAt,
                Quantity = (int)row.Quantity,
                IsSingleTicket = row.Quantity == 1,
                RemainingQuantity = (int)row.RemainingQuantity,
                FeatureIds = row.FeatureIds,
                // 목록에서는 이미지 생략
                TicketImages = new List<string>(),
                Seller = new SellerInfoReadModel
                {
                    UserId = (int)row.UserId,
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

        // feature_ids 파싱 및 feature 데이터 로드
        await LoadTicketFeaturesAsync(tickets);

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
            TicketId = (int)ticketRow.TicketId,
            // 좌석 등급 정보 (확장)
            SeatGradeId = ticketRow.SeatGradeId != null ? (int?)Convert.ToInt32(ticketRow.SeatGradeId) : null,
            SeatGradeCode = ticketRow.SeatGradeCode,
            SeatGradeName = ticketRow.SeatGradeName,
            SeatGradeNameEn = ticketRow.SeatGradeNameEn,
            SeatGradeSortOrder = ticketRow.SeatGradeSortOrder != null ? (int?)Convert.ToInt32(ticketRow.SeatGradeSortOrder) : null,
            // 구역 정보 (확장)
            AreaId = ticketRow.AreaId != null ? (int?)Convert.ToInt32(ticketRow.AreaId) : null,
            Area = ticketRow.Area,
            AreaSortOrder = ticketRow.AreaSortOrder != null ? (int?)Convert.ToInt32(ticketRow.AreaSortOrder) : null,
            // 위치 정보 (NEW)
            LocationId = ticketRow.LocationId != null ? (int?)Convert.ToInt32(ticketRow.LocationId) : null,
            LocationName = ticketRow.LocationName,
            LocationSortOrder = ticketRow.LocationSortOrder != null ? (int?)Convert.ToInt32(ticketRow.LocationSortOrder) : null,
            // 기존 필드
            Row = ticketRow.Row,
            Price = (int)ticketRow.Price,
            OriginalPrice = (int)ticketRow.OriginalPrice,
            IsConsecutive = ticketRow.IsConsecutive,
            TradeMethodId = ticketRow.TradeMethodId != null ? (int?)Convert.ToInt32(ticketRow.TradeMethodId) : null,
            TradeMethodName = ticketRow.TradeMethodName,
            HasTicket = ticketRow.HasTicket,
            Description = ticketRow.Description,
            CreatedAt = ticketRow.CreatedAt,
            Quantity = (int)ticketRow.Quantity,
            IsSingleTicket = ticketRow.Quantity == 1,
            RemainingQuantity = (int)ticketRow.RemainingQuantity,
            TicketImages = ticketImages.ToList(),
            Seller = new SellerInfoReadModel
            {
                UserId = (int)ticketRow.UserId,
                Nickname = ticketRow.Nickname,
                ProfileImageUrl = ticketRow.ProfileImageUrl,
                MannerTemperature = ticketRow.MannerTemperature != null ? (float?)Convert.ToDouble(ticketRow.MannerTemperature) : null,
                TotalTradeCount = ticketRow.TotalTradeCount != null ? Convert.ToInt32(ticketRow.TotalTradeCount) : 0,
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
    
    /// <summary>
    /// 티켓 목록의 특이사항을 배치로 로드하는 헬퍼 메서드
    /// </summary>
    private async Task LoadTicketFeaturesAsync(List<TicketListReadModel> tickets)
    {
        // feature_ids가 있는 티켓만 필터링
        var ticketsWithFeatures = tickets.Where(t => !string.IsNullOrWhiteSpace(t.FeatureIds)).ToList();
        
        if (!ticketsWithFeatures.Any())
        {
            return;
        }
        
        // 모든 unique feature ID 수집
        var allFeatureIds = ticketsWithFeatures
            .SelectMany(t => t.FeatureIds!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(int.Parse)
            .Distinct()
            .ToList();
        
        if (!allFeatureIds.Any())
        {
            return;
        }
        
        // 한 번의 쿼리로 모든 feature 정보 조회
        var allFeatures = await dapper.QueryAsync<TicketFeatureReadModel>(
            TicketQueries.GetTicketFeaturesByIds,
            new { Ids = allFeatureIds }
        );
        
        var featureDict = allFeatures.ToDictionary(f => f.FeatureId);
        
        // 각 티켓에 해당하는 features 할당
        foreach (var ticket in ticketsWithFeatures)
        {
            var ids = ticket.FeatureIds!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(int.Parse)
                                       .ToList();
            
            ticket.Features = ids
                .Where(id => featureDict.ContainsKey(id))
                .Select(id => featureDict[id])
                .ToList();
        }
    }
}

