using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.Application.Features.Migrations.Commands.ApplyMigrations;

public class ApplyMigrationsHandler : IRequestHandler<ApplyMigrationsCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public ApplyMigrationsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task Handle(ApplyMigrationsCommand request, CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
    }
}