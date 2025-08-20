using System.Text.RegularExpressions;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;

namespace Translator.Application.Helpers;

public static class LanguageDetector
{
    private static readonly Regex RangeRegex = new(@"^([0-9A-Fa-f]{4,6})-([0-9A-Fa-f]{4,6})$", RegexOptions.Compiled);
    
    private static readonly Dictionary<string, List<(int Start, int End)>> RangeCache = new();

    public static Language DetectOrThrow(string input, IEnumerable<Language> languages)
    {
        if (string.IsNullOrEmpty(input))
            throw new UknownLanguageException("Empty input");

        char firstChar = input[0];
        
        Language? detectedLanguage = null;
        List<(int Start, int End)>? detectedRanges = null;
        
        foreach (var language in languages)
        {
            var ranges = GetCachedRanges(language.UnicodeRange);
            if (IsInRanges(firstChar, ranges))
            {
                detectedLanguage = language;
                detectedRanges = ranges;
                break;
            }
        }

        if (detectedLanguage == null)
            throw new UknownLanguageException($"Unknown language for character: {firstChar}");

        for (int i = 1; i < input.Length; i++)
        {
            char currentChar = input[i];
            
            if (IsNeutralCharacter(currentChar))
                continue;
                
            if (!IsInRanges(currentChar, detectedRanges ?? []))
                throw new UknownLanguageException(
                    $"Mixed languages detected. Expected {detectedLanguage.Name}, but found character '{currentChar}' at position {i}");
        }

        return detectedLanguage;
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