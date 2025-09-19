using System.Text.RegularExpressions;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;

namespace Translator.Application.Helpers;

public static class LanguageDetector
{
    private static readonly Regex RangeRegex = new(@"^([0-9A-Fa-f]{4,6})-([0-9A-Fa-f]{4,6})$", RegexOptions.Compiled);

    private static readonly Dictionary<string, List<(int Start, int End)>> RangeCache = new();

    public static List<Language> DetectLanguages(string input, IEnumerable<Language> languages)
    {
        if (string.IsNullOrEmpty(input))
            throw new UnkownLanguageException("Empty input");

        var activeLanguages = languages.Where(l => l.IsActive).ToList();
        if (activeLanguages.Count == 0)
            return new List<Language>();

        var significantChars = input.Where(c => !IsNeutralCharacter(c)).ToList();

        if (significantChars.Count == 0)
            return new List<Language>();

        var candidateLanguages = new List<Language>();

        foreach (var language in activeLanguages)
        {
            var ranges = GetCachedRanges(language.UnicodeRange);

            var allCharsMatch = significantChars.All(c => IsInRanges(c, ranges));

            if (allCharsMatch)
                candidateLanguages.Add(language);
        }

        return candidateLanguages;
    }

    private static List<(int Start, int End)> GetCachedRanges(string rangeString)
    {
        if (RangeCache.TryGetValue(rangeString, out var cached))
            return cached;

        var ranges = ParseRanges(rangeString);
        RangeCache[rangeString] = ranges;
        return ranges;
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

    private static bool IsNeutralCharacter(char c)
    {
        return char.IsWhiteSpace(c) ||
               char.IsPunctuation(c) ||
               char.IsDigit(c) ||
               char.IsSymbol(c);
    }
}