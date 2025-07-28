using System.Text.Json.Serialization;

namespace Translator.Domain.Enums;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedLanguage
{
    ka,
    en,
    ru
}
