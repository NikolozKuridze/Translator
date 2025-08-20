using MediatR;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.SeedData;

namespace Translator.Application.Features.Migrations.Commands.SeedFakeData;

public class SeedFakeDataHandler : IRequestHandler<SeedFakeDataCommand>
{
    private readonly ApplicationDbContext _context;

    public SeedFakeDataHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public Task Handle(SeedFakeDataCommand request, CancellationToken cancellationToken)
    {
        var databaseSeeder = new DatabaseSeeder(_context);
        return databaseSeeder.SeedTranslationsAsync();
    }
}