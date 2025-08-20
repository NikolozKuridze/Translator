using Microsoft.EntityFrameworkCore;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.SeedData;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedTranslationsAsync()
    {
        Console.WriteLine("Data Generation...");
        
        var (templates, values, translations) = 
            TranslationDataSeeder.GenerateTranslationData(120_000); 
        
        Console.WriteLine($"Create: {templates.Count} templates, " +
                          $"{values.Count} values, " +
                          $"{translations.Count} translations");

        await _context.Templates.AddRangeAsync(templates);
        await _context.SaveChangesAsync();
        Console.WriteLine("Templates saved");
        
        for (int i = 0; i < values.Count; i += 10_000)
        {
            var batch = values.Skip(i).Take(10_000);
            await _context.Values.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Added {Math.Min(i + 10_000, values.Count)} of {values.Count} values");
        }
        
        await AssignValuesToTemplates();
        Console.WriteLine("Template-Value relationships created");
        
        for (int i = 0; i < translations.Count; i += 10_000)
        {
            var batch = translations.Skip(i).Take(10_000);
            await _context.Translations.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Added {Math.Min(i + 10_000, translations.Count)} of {translations.Count} translations");
        }
        
        Console.WriteLine("Done");
    }

    private async Task AssignValuesToTemplates()
    {
        var templates = await _context.Templates.ToListAsync();
        var values = await _context.Values.ToListAsync();
        
        for (int i = 0; i < values.Count; i += 10_000)
        {
            var batch = values.Skip(i).Take(10_000).ToList();
            var templateIndex = i; 
            
            foreach (var value in batch)
            {
                var template = templates[templateIndex % templates.Count];
                var templateProperty = typeof(Value).GetProperty("TemplateId");
                
                if (templateProperty != null && templateProperty.CanWrite)
                    templateProperty.SetValue(value, template.Id);
                else
                    template.AddValue(value);

                templateIndex++;
            }
            
            await _context.SaveChangesAsync();
            Console.WriteLine($"Assigned {Math.Min(i + 10_000, values.Count)} values to templates");
        }
    }
}
