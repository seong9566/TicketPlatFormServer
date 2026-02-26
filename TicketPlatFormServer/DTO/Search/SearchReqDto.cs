namespace TicketPlatFormServer.DTO.Search;

public class SearchReqDto
{
    public string Keyword { get; set; } = string.Empty;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
