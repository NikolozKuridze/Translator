using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Configurations;

namespace Translator.Infrastructure.Database.Postgres.SeedData;

public class LanguageSeeder
{
    private readonly LanguageSeedingConfiguration _languageSeedingConfiguration;

    public LanguageSeeder(LanguageSeedingConfiguration languageSeedingConfiguration)
    {
        _languageSeedingConfiguration = languageSeedingConfiguration;
    }
    public async Task Seed(ModelBuilder modelBuilder)
    {

        var path = Path.Combine(AppContext.BaseDirectory, _languageSeedingConfiguration.Path);
        var json = File.ReadAllText(path);

        var items = JsonSerializer.Deserialize<List<LanguageSeedDto>>(json)!;

        var languages = items.Select(l => new Language(l.code, l.name, $"{l.hexrange.First()}-{l.hexrange.Last()}")
        {
            Id = StaticGuidGenerator.CreateGuidFromString(l.name),
            IsActive = false
        });
        
        modelBuilder.Entity<Language>().HasData(languages);
    }

    private record LanguageSeedDto(string code, string name, List<string> hexrange);
}