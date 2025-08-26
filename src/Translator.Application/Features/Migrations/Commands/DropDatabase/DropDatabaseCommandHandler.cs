using MediatR;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Migrations.Commands.DropDatabase;

public class DropDatabaseCommandHandler : IRequestHandler<DropDatabaseCommand>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly LogsDbContext _logsDbContext;
    
    public DropDatabaseCommandHandler(
        ApplicationDbContext applicationDbContext,
        LogsDbContext logsDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _logsDbContext = logsDbContext;
    }
    
    public async Task Handle(DropDatabaseCommand request, CancellationToken cancellationToken)
    {
        await _applicationDbContext.Database.EnsureDeletedAsync(cancellationToken);
        await _logsDbContext.Database.EnsureDeletedAsync(cancellationToken);
    }
}