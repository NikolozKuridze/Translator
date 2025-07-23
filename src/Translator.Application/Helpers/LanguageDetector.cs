using Translator.Domain.Enums;

namespace Translator.Application.Helpers;

public static class LanguageDetector
{
    // TODO: check all symbols
    public static Languages DetectLanguage(string input)
    {
        foreach (char c in input)
        {
            if (IsCyrillic(c))
                return Languages.Ru;

            if (IsGeorgian(c))
                return Languages.Ge;

            if (IsLatin(c))
                return Languages.En;
        }

        return Languages.En;
    }

    private static bool IsCyrillic(char c) =>
        (c >= '\u0400' && c <= '\u04FF') || (c >= '\u0500' && c <= '\u052F');

    private static bool IsGeorgian(char c) =>
        c >= '\u10A0' && c <= '\u10FF';

    private static bool IsLatin(char c) =>
        (c >= '\u0041' && c <= '\u007A') || (c >= '\u00C0' && c <= '\u00FF'); 
}