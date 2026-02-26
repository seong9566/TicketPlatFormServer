using TicketPlatFormServer.DTO.Search;

namespace TicketPlatFormServer.Services.Search;

public interface ISearchService
{
    Task<SearchResDto> SearchAsync(string keyword, int page, int pageSize);
}
