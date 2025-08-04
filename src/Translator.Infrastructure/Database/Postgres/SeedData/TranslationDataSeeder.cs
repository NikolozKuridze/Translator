using Bogus;
using System.Security.Cryptography;
using System.Text;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.SeedData;

public class TranslationDataSeeder
{
    private static readonly Guid RussianLanguageId = new("2069bade-06e7-4015-1385-fe1fb5a379ec");
    private static readonly Guid EnglishLanguageId = new("383a4678-5a4a-faa4-d5fa-73e2f506ecfc");

    private const int MAX_TEMPLATE_NAME_LENGTH = 100;
    private const int MAX_KEY_LENGTH = 100;
    private const int MAX_TRANSLATION_VALUE_LENGTH = 500;

    private static readonly string[] ShortTemplateCategories = {
        "UI", "Forms", "Buttons", "Messages", "Errors", "Products", 
        "Help", "User", "Auth", "Settings", "Navigation"
    };

    private static readonly string[] ShortKeyPatterns = {
        "btn.{0}", "lbl.{0}", "msg.{0}", "err.{0}", "tip.{0}",
        "fld.{0}", "page.{0}", "menu.{0}", "act.{0}", "status.{0}"
    };

    public static (List<Template> templates, List<Value> values, List<Translation> translations)
        GenerateTranslationData(int translationsPerLanguage = 120_000)
    {
        var templates = new List<Template>();
        var values = new List<Value>();
        var translations = new List<Translation>();

        templates = GenerateTemplates(200);
        Console.WriteLine($"Generated {templates.Count} templates");

        values = GenerateValues(translationsPerLanguage);
        Console.WriteLine($"Generated {values.Count} values");

        translations = GenerateTranslations(values);
        Console.WriteLine($"Generated {translations.Count} translations");

        return (templates, values, translations);
    }

    private static List<Template> GenerateTemplates(int templateCount)
    {
        var templates = new List<Template>();
        var templateNames = new HashSet<string>();
        var templateFaker = new Faker();

        while (templates.Count < templateCount)
        {
            var name = $"{templateFaker.PickRandom(ShortTemplateCategories)}.{templateFaker.Commerce.Department().Replace(" ", "")}";
            if (name.Length > MAX_TEMPLATE_NAME_LENGTH) 
                name = name[..MAX_TEMPLATE_NAME_LENGTH];
                
            if (templateNames.Add(name))
                templates.Add(new Template(name));
        }

        return templates;
    }

    private static List<Value> GenerateValues(int valueCount)
    {
        var values = new List<Value>();
        var faker = new Faker();
        
        for (int i = 0; i < valueCount; i++)
        {
            var pattern = faker.PickRandom(ShortKeyPatterns);
            var keyName = faker.Hacker.Noun()
                .Replace(" ", "")
                .Replace("-", "")
                .ToLowerInvariant();

            var key = string.Format(pattern, keyName) + "_" + i;
            if (key.Length > MAX_KEY_LENGTH) 
                key = key[..MAX_KEY_LENGTH];

            values.Add(new Value(key));

            if ((i + 1) % 50_000 == 0)
                Console.WriteLine($"Values progress: {i + 1}/{valueCount}");
        }

        return values;
    }

    private static List<Translation> GenerateTranslations(List<Value> values)
    {
        var translations = new List<Translation>();
        var faker = new Faker();

        Console.WriteLine("Generating Russian translations");
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            var translationText = GenerateRussianTranslation(faker, value.Key);
            if (translationText.Length > MAX_TRANSLATION_VALUE_LENGTH)
                translationText = translationText[..MAX_TRANSLATION_VALUE_LENGTH];

            var translation = new Translation(value.Id, translationText);
            SetLanguageId(translation, RussianLanguageId);
            translations.Add(translation);

            if ((i + 1) % 50_000 == 0)
                Console.WriteLine($"RU translations progress: {i + 1}/{values.Count}");
        }

        Console.WriteLine("Generating English translations");
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            var translationText = GenerateEnglishTranslation(faker, value.Key);
            if (translationText.Length > MAX_TRANSLATION_VALUE_LENGTH)
                translationText = translationText[..MAX_TRANSLATION_VALUE_LENGTH];

            var translation = new Translation(value.Id, translationText);
            SetLanguageId(translation, EnglishLanguageId);
            translations.Add(translation);

            if ((i + 1) % 50_000 == 0)
                Console.WriteLine($"EN translations progress: {i + 1}/{values.Count}");
        }

        return translations;
    }

    private static string GenerateRussianTranslation(Faker faker, string key)
    {
        return key.ToLowerInvariant() switch
        {
            var k when k.Contains("btn") => faker.PickRandom("Сохранить", "Отменить", "Удалить", "Создать", "Закрыть"),
            var k when k.Contains("err") => $"Ошибка: {faker.Lorem.Sentence(2, 4)}",
            var k when k.Contains("msg") => faker.Lorem.Sentence(3, 6),
            var k when k.Contains("lbl") => faker.Commerce.ProductName(),
            _ => faker.Lorem.Sentence(2, 5)
        };
    }

    private static string GenerateEnglishTranslation(Faker faker, string key)
    {
        return key.ToLowerInvariant() switch
        {
            var k when k.Contains("btn") => faker.PickRandom("Save", "Cancel", "Delete", "Create", "Close"),
            var k when k.Contains("err") => $"Error: {faker.Lorem.Sentence(2, 4)}",
            var k when k.Contains("msg") => faker.Lorem.Sentence(3, 6),
            var k when k.Contains("lbl") => faker.Commerce.ProductName(),
            _ => faker.Lorem.Sentence(2, 5)
        };
    }

    private static void SetLanguageId(Translation translation, Guid languageId)
    {
        var propertyInfo = typeof(Translation).GetProperty("LanguageId");
        propertyInfo?.SetValue(translation, languageId);
    }
}
