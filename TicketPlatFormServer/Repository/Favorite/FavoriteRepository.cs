using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Favorite;

/// <summary>
/// 찜 관련 Repository 구현체 (Primary Constructor 패턴)
/// </summary>
public class FavoriteRepository(
    TicketContext db,
    IDbConnection dapper,
    ILogger<FavoriteRepository> logger) : IFavoriteRepository
{
    /// <summary>
    /// 찜 토글 (이미 찜한 경우 삭제, 아니면 추가)
    /// </summary>
    public async Task<bool> ToggleFavorite(int userId, int favoriteTypeId, int targetId)
    {
        // 기존 찜 여부 확인
        var existingFavorite = await dapper.QueryFirstOrDefaultAsync<int?>(
            FavoriteQueries.CheckFavoriteExists,
            new { UserId = userId, FavoriteTypeId = favoriteTypeId, TargetId = targetId }
        );

        if (existingFavorite.HasValue)
        {
            // 이미 찜한 경우 → 삭제
            await dapper.ExecuteAsync(
                FavoriteQueries.DeleteFavorite,
                new { UserId = userId, FavoriteTypeId = favoriteTypeId, TargetId = targetId }
            );

            logger.LogInformation(
                "[FavoriteRepository.ToggleFavorite] 찜 삭제 | UserId: {UserId}, TypeId: {TypeId}, TargetId: {TargetId}",
                userId, favoriteTypeId, targetId
            );

            return false; // 삭제됨
        }
        else
        {
            // 찜하지 않은 경우 → 추가
            await dapper.ExecuteAsync(
                FavoriteQueries.InsertFavorite,
                new { UserId = userId, FavoriteTypeId = favoriteTypeId, TargetId = targetId }
            );

            logger.LogInformation(
                "[FavoriteRepository.ToggleFavorite] 찜 추가 | UserId: {UserId}, TypeId: {TypeId}, TargetId: {TargetId}",
                userId, favoriteTypeId, targetId
            );

            return true; // 추가됨
        }
    }

    /// <summary>
    /// 티켓 존재 및 판매 가능 여부 확인
    /// </summary>
    public async Task<bool> CheckTicketExists(int ticketId)
    {
        var exists = await dapper.QueryFirstOrDefaultAsync<int?>(
            FavoriteQueries.CheckTicketExists,
            new { TicketId = ticketId }
        );

        return exists.HasValue;
    }

    /// <summary>
    /// 사용자가 찜한 티켓 목록 조회 (이벤트 정보 포함)
    /// </summary>
    public async Task<List<FavoriteTicketReadModel>> GetFavoriteTicketsByUserId(int userId, int favoriteTypeId)
    {
        var rows = await dapper.QueryAsync<dynamic>(
            FavoriteQueries.GetFavoriteTicketsByUserId,
            new { UserId = userId, FavoriteTypeId = favoriteTypeId }
        );

        var result = new List<FavoriteTicketReadModel>();

        foreach (var row in rows)
        {
            result.Add(new FavoriteTicketReadModel
            {
                TicketId = row.TicketId,
                SeatGradeId = row.SeatGradeId,
                SeatGradeName = row.SeatGradeName,
                Area = row.Area,
                Row = row.Row,
                Price = row.Price,
                OriginalPrice = row.OriginalPrice,
                RemainingQuantity = row.RemainingQuantity,
                IsConsecutive = row.IsConsecutive,
                TradeMethodId = row.TradeMethodId,
                TradeMethodName = row.TradeMethodName,
                HasTicket = row.HasTicket,
                CreatedAt = row.CreatedAt,
                FavoritedAt = row.FavoritedAt,
                EventTitle = row.EventTitle,
                EventDate = row.EventDate,
                VenueName = row.VenueName,
                EventPosterImageUrl = row.EventPosterImageUrl,
                Seller = new SellerInfoReadModel
                {
                    UserId = row.SellerId,
                    Nickname = row.Nickname,
                    ProfileImageUrl = row.ProfileImageUrl,
                    MannerTemperature = row.MannerTemperature != null ? (float?)Convert.ToDouble(row.MannerTemperature) : null,
                    TotalTradeCount = row.TotalTradeCount ?? 0,
                    ResponseRate = row.ResponseRate != null ? (float?)Convert.ToDouble(row.ResponseRate) : null,
                    IsSecurePayment = row.IsSecurePayment == 1
                }
            });
        }

        return result;
    }

    /// <summary>
    /// 특정 티켓의 찜 여부 확인
    /// </summary>
    public async Task<bool> CheckIsFavorited(int userId, int favoriteTypeId, int ticketId)
    {
        var favoriteId = await dapper.QueryFirstOrDefaultAsync<int?>(
            FavoriteQueries.CheckFavoriteExists,
            new { UserId = userId, FavoriteTypeId = favoriteTypeId, TargetId = ticketId }
        );

        return favoriteId.HasValue;
    }
}
