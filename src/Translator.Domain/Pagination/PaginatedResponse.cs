namespace Translator.Domain.Pagination;

public class PaginatedResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool IsFirstPage => Page == 1;
    public bool IsLastPage => Page == TotalPages;
}