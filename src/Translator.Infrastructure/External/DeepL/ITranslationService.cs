namespace Translator.Infrastructure.External.DeepL;

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string targetLanguageCode, CancellationToken ct = default);
}