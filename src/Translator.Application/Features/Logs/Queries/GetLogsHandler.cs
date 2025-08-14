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
       var logsCount = await _dbContext.Logs.CountAsync(cancellationToken); 
        
        return await _dbContext
            .Logs
            .Skip(request.Skip)
            .Take(request.Page)
            .Select(t => new GetLogsResponse(
                    t.Message, t.Timestamp, t.Exception, t.LogEvent, logsCount
                ))
            .ToArrayAsync(cancellationToken);
    }
}

public record GetLogsResponse(
    string Message, 
    DateTimeOffset Timestamp, 
    string? Exception,
    string? Properties,
    int LogsCount);