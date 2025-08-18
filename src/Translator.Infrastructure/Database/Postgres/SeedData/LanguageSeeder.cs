using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.SeedData;

public static class LanguageSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Database", "Postgres", "SeedData", "languages.json");
        var json = File.ReadAllText(path);

        var items = JsonSerializer.Deserialize<List<LanguageSeedDto>>(json)!;

        var languages = items.Select(l => new Language(l.Code, l.Name, $"{l.HexRange[0]}-{l.HexRange[1]}")
        {
            Id = StaticGuidGenerator.CreateGuidFromString(l.Name),
            IsActive = false
        });
        modelBuilder.Entity<Language>().HasData(languages);
    }

    private class LanguageSeedDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("hexrange")]
        public List<string> HexRange { get; set; } = new();
    }
}