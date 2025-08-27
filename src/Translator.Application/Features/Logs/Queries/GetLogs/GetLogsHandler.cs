using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Logs.Queries.GetLogs;

public class GetLogsHandler : IRequestHandler<GetLogsCommand, PaginatedResponse<GetLogsResponse>>
{
    private readonly LogsDbContext _dbContext;

    public GetLogsHandler(LogsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<PaginatedResponse<GetLogsResponse>> Handle(GetLogsCommand request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Logs.AsQueryable();
    
        if (request.Pagination.DateFrom.HasValue)
            query = query.Where(t => t.Timestamp >= request.Pagination.DateFrom.Value);
    
        if (request.Pagination.DateTo.HasValue)
            query = query.Where(t => t.Timestamp <= request.Pagination.DateTo.Value);
    
        if (request.Level.HasValue)
            query = query.Where(t => t.Level == request.Level.Value);
    
        query = query.OrderByDescending(t => t.Timestamp);
    
        var totalCount = await query.CountAsync(cancellationToken);
    
        var items = await query
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .Select(t => new GetLogsResponse(
                t.Id,
                t.Message, 
                t.Level,
                t.Timestamp, 
                t.Exception, 
                t.LogEvent
            ))
            .ToArrayAsync(cancellationToken);

        return new PaginatedResponse<GetLogsResponse>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = totalCount,
            HasNextPage = (request.Pagination.Page * request.Pagination.PageSize) < totalCount,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = items
        };
    }
}

public record GetLogsResponse(
    long Id,
    string Message, 
    int Level,
    DateTimeOffset Timestamp, 
    string? Exception,
    string? Properties);