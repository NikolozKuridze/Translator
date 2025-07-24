using System.Text.RegularExpressions;
using Translator.Application.Exceptions;
using Translator.Domain.DataModels;

namespace Translator.Application.Helpers;

public static class LanguageDetector
{
    private static readonly Regex RangeRegex = new(@"^([0-9A-Fa-f]{4,6})-([0-9A-Fa-f]{4,6})$", RegexOptions.Compiled);

    public static Language DetectOrThrow(string input, IEnumerable<Language> languages)
    {
        foreach (var language in languages)
        {
            var ranges = ParseRanges(language.UnicodeRange);
            if (input.All(c => IsInRanges(c, ranges)))
                return language;
        }

        throw new UknownLanguageException(input);
    }

    private static List<(int Start, int End)> ParseRanges(string rangeString)
    {
        return rangeString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(range =>
            {
                var match = RangeRegex.Match(range.Trim());
                if (!match.Success)
                    throw new Exception($"Invalid range: {range}");

                return (
                    Convert.ToInt32(match.Groups[1].Value, 16),
                    Convert.ToInt32(match.Groups[2].Value, 16)
                );
            })
            .ToList();
    }

    private static bool IsInRanges(char c, List<(int Start, int End)> ranges)
    {
        int code = c;
        return ranges.Any(r => code >= r.Start && code <= r.End);
    }
}