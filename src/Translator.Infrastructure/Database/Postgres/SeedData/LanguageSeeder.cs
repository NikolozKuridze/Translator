using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Translator.Domain.DataModels;
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