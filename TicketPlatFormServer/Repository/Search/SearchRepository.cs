using System.Data;
using Dapper;
using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Search;

public class SearchRepository(IDbConnection dapper) : ISearchRepository
{
    private const string SearchEventsSql = @"
        SELECT
            e.id AS EventId,
            e.title AS Title,
            e.start_at AS EventDate,
            e.venue_name AS Location,
            e.poster_image_url AS ImageUrl,
            MIN(t.price) AS MinPrice
        FROM events e
        LEFT JOIN tickets t ON e.id = t.event_id
            AND t.deleted_at IS NULL
            AND t.status_id = 1
            AND t.remaining_quantity > 0
        WHERE e.title LIKE @KeywordLike
          AND e.is_active = 1
        GROUP BY e.id, e.title, e.start_at, e.venue_name, e.poster_image_url
        ORDER BY e.start_at ASC, e.id DESC
        LIMIT @Limit OFFSET @Offset";

    private const string CountEventsSql = @"
        SELECT COUNT(1)
        FROM events e
        WHERE e.title LIKE @KeywordLike
          AND e.is_active = 1";

    private const string SearchTicketsSql = @"
        SELECT
            t.id AS TicketId,
            COALESCE(t.event_id, 0) AS EventId,
            e.title AS EventTitle,
            t.price AS Price,
            TRIM(CONCAT_WS(' ', esg.name_ko, esa.area_name, t.`row`)) AS SeatInfo,
            CAST(t.status_id AS CHAR) AS Status
        FROM tickets t
        INNER JOIN events e ON t.event_id = e.id
        LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
        LEFT JOIN event_seat_areas esa ON t.area_id = esa.id
        WHERE t.deleted_at IS NULL
          AND e.is_active = 1
          AND (
              e.title LIKE @KeywordLike
              OR t.description LIKE @KeywordLike
          )
        ORDER BY t.created_at DESC, t.id DESC
        LIMIT @Limit OFFSET @Offset";

    private const string CountTicketsSql = @"
        SELECT COUNT(1)
        FROM tickets t
        INNER JOIN events e ON t.event_id = e.id
        WHERE t.deleted_at IS NULL
          AND e.is_active = 1
          AND (
              e.title LIKE @KeywordLike
              OR t.description LIKE @KeywordLike
          )";

    public async Task<SearchResultReadModel> SearchAsync(string keyword, int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;
        var parameters = new
        {
            KeywordLike = $"%{keyword}%",
            Limit = pageSize,
            Offset = offset
        };

        var events = await dapper.QueryAsync<SearchEventReadModel>(SearchEventsSql, parameters);
        var eventTotalCount = await dapper.QuerySingleAsync<int>(CountEventsSql, parameters);
        var tickets = await dapper.QueryAsync<SearchTicketReadModel>(SearchTicketsSql, parameters);
        var ticketTotalCount = await dapper.QuerySingleAsync<int>(CountTicketsSql, parameters);

        return new SearchResultReadModel
        {
            Events = events.ToList(),
            Tickets = tickets.ToList(),
            EventTotalCount = eventTotalCount,
            TicketTotalCount = ticketTotalCount
        };
    }
}
