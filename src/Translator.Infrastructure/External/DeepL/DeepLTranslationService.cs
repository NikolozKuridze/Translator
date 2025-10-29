using DeepL;

namespace Translator.Infrastructure.External.DeepL;

public class DeepLTranslationService : ITranslationService
{
    private readonly DeepLClient _client;

    public DeepLTranslationService(DeepLClient client) =>
        _client = client ?? throw new ArgumentNullException(nameof(client));

    public async Task<string> TranslateAsync(
        string text,
        string targetLanguageCode,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty.", nameof(text));
        if (string.IsNullOrWhiteSpace(targetLanguageCode))
            throw new ArgumentException("Target language code is required.", nameof(targetLanguageCode));

        var options = new TextTranslateOptions
        {
            PreserveFormatting = true,
            SentenceSplittingMode = SentenceSplittingMode.NoNewlines,
            Formality = Formality.More
        };

        var result = await _client.TranslateTextAsync(
            text,
            sourceLanguageCode: null,
            targetLanguageCode: targetLanguageCode.Trim(),
            options,
            cancellationToken: ct);

        return result.Text.Trim();
    }
}