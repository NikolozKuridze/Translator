using MediatR;
using Microsoft.Extensions.Options;
using Translator.Infrastructure.Configurations;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.SeedData;

namespace Translator.Application.Features.Migrations.Commands.SeedLanguages;

public class SeedLanguagesHandler : IRequestHandler<SeedLanguagesCommand>
{
    private readonly IOptions<LanguageSeedingConfiguration> _languageSeedingConfiguration;
    private readonly ApplicationDbContext _dbContext;

    public SeedLanguagesHandler(
        IOptions<LanguageSeedingConfiguration> languageSeedingConfiguration,
        ApplicationDbContext dbContext)
    {
        _languageSeedingConfiguration = languageSeedingConfiguration;
        _dbContext = dbContext;
    }
    
    public async Task Handle(SeedLanguagesCommand request, CancellationToken cancellationToken)
    {
        var seeder = new LanguageSeeder(_languageSeedingConfiguration.Value);
        await seeder.Seed(_dbContext);
    }
}