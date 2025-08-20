namespace Translator.Domain.Pagination;

public class PaginationRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }

    public PaginationRequest(
        int page = 1,
        int pageSize = 10,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? search = null,
        string? sortBy = null,
        string? sortDirection = "asc")
    {
        Page = page;
        PageSize = pageSize;
        Search = search;
        DateFrom = dateFrom;
        DateTo = dateTo;
        SortBy = sortBy;
        SortDirection = sortDirection;
    }
}