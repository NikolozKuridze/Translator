using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Logs.Queries;

public class GetLogsHandler : IRequestHandler<GetLogsCommand, IEnumerable<GetLogsResponse>>
{
    private readonly LogsDbContext _dbContext;

    public GetLogsHandler(LogsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<GetLogsResponse>> Handle(GetLogsCommand request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Logs.AsQueryable();
    
        if (request.DateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= request.DateFrom.Value);
    
        if (request.DateTo.HasValue)
            query = query.Where(l => l.Timestamp <= request.DateTo.Value);
    
        var logsCount = await query.CountAsync(cancellationToken);
    
        var results = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(t => new GetLogsResponse(
                t.Message, 
                t.Timestamp, 
                t.Exception, 
                t.LogEvent, 
                logsCount
            ))
            .ToArrayAsync(cancellationToken);
    
        return results;
    }
}

public record GetLogsResponse(
    string Message, 
    DateTimeOffset Timestamp, 
    string? Exception,
    string? Properties,
    int LogsCount);