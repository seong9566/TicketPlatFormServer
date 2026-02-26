using TicketPlatFormServer.Repository.ReadModels;

namespace TicketPlatFormServer.Repository.Search;

public interface ISearchRepository
{
    Task<SearchResultReadModel> SearchAsync(string keyword, int page, int pageSize);
}
