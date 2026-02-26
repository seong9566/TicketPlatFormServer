using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Search;
using TicketPlatFormServer.Repository.Search;

namespace TicketPlatFormServer.Services.Search;

public class SearchService(ISearchRepository searchRepository) : ISearchService
{
    public async Task<SearchResDto> SearchAsync(string keyword, int page, int pageSize)
    {
        var trimmedKeyword = keyword?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedKeyword))
        {
            throw new AppException("검색어를 입력해주세요.", HttpStatusCode.BadRequest);
        }

        if (page <= 0)
        {
            throw new AppException("페이지 번호는 1 이상이어야 합니다.", HttpStatusCode.BadRequest);
        }

        if (pageSize <= 0)
        {
            throw new AppException("페이지 크기는 1 이상이어야 합니다.", HttpStatusCode.BadRequest);
        }

        var readModel = await searchRepository.SearchAsync(trimmedKeyword, page, pageSize);

        return new SearchResDto
        {
            Events = readModel.Events.Select(x => new SearchEventItemDto
            {
                EventId = x.EventId,
                Title = x.Title,
                EventDate = x.EventDate,
                Location = x.Location,
                ImageUrl = x.ImageUrl,
                MinPrice = x.MinPrice
            }).ToList(),
            Tickets = readModel.Tickets.Select(x => new SearchTicketItemDto
            {
                TicketId = x.TicketId,
                EventId = x.EventId,
                EventTitle = x.EventTitle,
                Price = x.Price,
                SeatInfo = x.SeatInfo,
                Status = x.Status
            }).ToList(),
            TotalCount = readModel.EventTotalCount + readModel.TicketTotalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
