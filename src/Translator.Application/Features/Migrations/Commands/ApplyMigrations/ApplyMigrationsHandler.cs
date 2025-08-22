using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Migrations.Commands.ApplyMigrations;

public class ApplyMigrationsHandler : IRequestHandler<ApplyMigrationsCommand>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly LogsDbContext _logsDbContext;

    public ApplyMigrationsHandler(
        ApplicationDbContext applicationDbContext,
        LogsDbContext logsDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _logsDbContext = logsDbContext;
    }
    
    public async Task Handle(ApplyMigrationsCommand request, CancellationToken cancellationToken)
    {
        await _applicationDbContext.Database.MigrateAsync(cancellationToken);
        await _logsDbContext.Database.MigrateAsync(cancellationToken);
    }
}