using System.Text.Json;
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

    public Task Seed(ModelBuilder modelBuilder)
    {
        var path = Path.Combine(AppContext.BaseDirectory, _languageSeedingConfiguration.Path);
        var json = File.ReadAllText(path);

        var items = JsonSerializer.Deserialize<List<LanguageSeedDto>>(json)!;

        var languages = items.Select(l => new Language(
            l.code,
            l.name,
            string.Join(";", Enumerable.Range(0, l.hexrange.Count / 2)
                .Select(i => $"{l.hexrange[i * 2]}-{l.hexrange[i * 2 + 1]}"))
        )
        {
            Id = StaticGuidGenerator.CreateGuidFromString(l.name),
            IsActive = l.code == "en",
        });

        foreach (var lang in languages)
        {
            Console.WriteLine($"{lang.Name}: {lang.UnicodeRange}");
        }

        modelBuilder.Entity<Language>().HasData(languages);
        return Task.CompletedTask;
    }

    private record LanguageSeedDto(string code, string name, List<string> hexrange);
}