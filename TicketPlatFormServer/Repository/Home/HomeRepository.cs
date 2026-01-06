using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DTO.Home;

namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// 홈 화면 Repository 구현체 (Primary Constructor + Static Class 패턴)
/// </summary>
public class HomeRepository(TicketContext context, IDbConnection dapper) : IHomeRepository
{
    public async Task<List<PopularTicketDto>> GetPopularTickets(int limit = 10)
    {
        var result = await dapper.QueryAsync<PopularTicketDto>(
            HomeQueries.GetPopularTickets,
            new { Limit = limit }
        );

        return result.ToList();
    }

    public async Task<List<RecommendedEventDto>> GetRecommendedEvents(int? userId = null, int limit = 5)
    {
        var sql = userId.HasValue
            ? HomeQueries.GetRecommendedEventsForUser
            : HomeQueries.GetRecommendedEventsForGuest;

        var result = await dapper.QueryAsync<RecommendedEventDto>(
            sql,
            new { UserId = userId, Limit = limit }
        );

        return result.ToList();
    }
}

