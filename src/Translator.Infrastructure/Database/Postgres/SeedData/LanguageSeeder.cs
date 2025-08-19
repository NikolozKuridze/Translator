using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    public Task Seed(ApplicationDbContext dbContext)
    {
        //Database/Postgres/SeedData/languages.json
        var path = Path.Combine(AppContext.BaseDirectory, _languageSeedingConfiguration.Path);
        var json = File.ReadAllText(path);

        var items = JsonSerializer.Deserialize<List<LanguageSeedDto>>(json)!;

        var languages = items.Select(l => new Language(l.Code, l.Name, $"{l.HexRange[0]}-{l.HexRange[1]}")
        {
            Id = StaticGuidGenerator.CreateGuidFromString(l.Name),
            IsActive = false
        });
        dbContext.Languages.AddRangeAsync(languages);
        return dbContext.SaveChangesAsync();
    }

    private class LanguageSeedDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        
        [JsonPropertyName("hexrange")]
        public List<string> HexRange { get; set; } = default!;
    }
}