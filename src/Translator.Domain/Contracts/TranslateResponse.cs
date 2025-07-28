using Translator.Domain.Enums;

namespace Translator.Domain.Contracts;

public class TranslateResponse
{
    public string OriginalText { get; set; } = string.Empty;
    public string DetectedLanguage { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public SupportedLanguage TargetLanguage { get; set; }
}
