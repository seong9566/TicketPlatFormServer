using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Search;
using TicketPlatFormServer.Services.Search;

namespace TicketPlatFormServer.Controllers.Search;

[ApiController]
[Route("api/search")]
public class SearchController(ISearchService searchService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchReqDto req)
    {
        var result = await searchService.SearchAsync(req.Keyword, req.Page, req.PageSize);

        var response = new ApiResponse<SearchResDto>(
            message: "검색 결과 조회 성공",
            data: result,
            statusCode: 200
        );

        return Ok(response);
    }
}
