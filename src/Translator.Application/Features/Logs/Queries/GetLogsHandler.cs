using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Logs.Queries;

public class GetLogsHandler : IRequestHandler<GetLogsCommand, IEnumerable<GetLogsResponse>>
{
    private readonly LogsDbContext _dbContext;
    private readonly ILogger<GetLogsHandler> _logger;

    public GetLogsHandler(LogsDbContext dbContext,
        ILogger<GetLogsHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<IEnumerable<GetLogsResponse>> Handle(GetLogsCommand request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Logs.AsQueryable();
    
        if (request is { DateFrom: not null, DateTo: not null })
            query = query.Where(
                l => l.Timestamp >= request.DateFrom.Value
                && l.Timestamp <= request.DateTo.Value);
   
        var logsCount = await query.CountAsync(cancellationToken);
    
        var results = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip(request.Skip)
            .Take(request.Page)
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