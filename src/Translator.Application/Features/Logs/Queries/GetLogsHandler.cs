using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Logs.Queries;

public class GetLogsHandler : IRequestHandler<GetLogsCommand, PaginatedResponse<GetLogsResponse>>
{
    private readonly LogsDbContext _dbContext;
    private readonly ILogger<GetLogsHandler> _logger;

    public GetLogsHandler(LogsDbContext dbContext, ILogger<GetLogsHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<GetLogsResponse>> Handle(GetLogsCommand request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Logs.AsQueryable();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        query = query.OrderByDescending(t => t.Timestamp);
        
        var items = await query
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .Select(t => new GetLogsResponse(
                t.Message, 
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
    string Message, 
    DateTimeOffset Timestamp, 
    string? Exception,
    string? Properties);