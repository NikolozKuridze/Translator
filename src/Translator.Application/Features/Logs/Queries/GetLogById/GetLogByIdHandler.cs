using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Features.Logs.Queries.GetLogs;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Logs.Queries.GetLogById;

public class GetLogByIdHandler : IRequestHandler<GetLogByIdCommand, GetLogsResponse?>
{
    private readonly LogsDbContext _dbContext;

    public GetLogByIdHandler(LogsDbContext dbContext)
        => _dbContext = dbContext;
    
    public async Task<GetLogsResponse?> Handle(GetLogByIdCommand request, CancellationToken cancellationToken)
    {
        var log = await _dbContext.Logs
            .Where(l => l.Id == request.Id)
            .Select(t => new GetLogsResponse(
                t.Id,
                t.Message,
                t.Level,
                t.Timestamp,
                t.Exception,
                t.LogEvent
            ))
            .SingleOrDefaultAsync(cancellationToken);

        return log;
    }
}