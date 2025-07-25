using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.SeedData;

public static class LanguageSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Database", "Postgres", "SeedData", "languages.json");
        var json = File.ReadAllText(path);

        var items = JsonSerializer.Deserialize<List<LanguageSeedDto>>(json)!;

        var languages = items.Select(l => new Language(l.code, l.Name, $"{l.hexrange[0]}-{l.hexrange[1]}")
        {
            Id = StaticGuidGenerator.CreateGuidFromString(l.Name),
            IsActive = false
        });
        modelBuilder.Entity<Language>().HasData(languages);
    }

    private class LanguageSeedDto
    {
        public string code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<string> hexrange { get; set; } = default!;
    }
}