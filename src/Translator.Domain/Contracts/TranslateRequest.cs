using Translator.Domain.Enums;

namespace Translator.Domain.Contracts;

public record TranslateRequest
{
    public string Text { get; set; } = string.Empty;
    public SupportedLanguage TargetLanguage { get; set; }
}
