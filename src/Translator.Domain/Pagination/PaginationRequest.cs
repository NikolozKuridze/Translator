namespace Translator.Domain.Pagination;

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }

    public PaginationRequest(
        int page,
        int pageSize,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? search,
        string? sortBy,
        string? sortDirection = "asc")
    {
        Page = page;
        PageSize = pageSize;
        Search = search;
        DateFrom = dateFrom;
        DateTo = dateTo;
        SortBy = sortBy?.ToLower();
        SortDirection = sortDirection;
    }
}