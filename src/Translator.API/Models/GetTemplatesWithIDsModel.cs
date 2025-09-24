namespace Translator.API.Models;

public sealed record GetTemplatesWithIDsModel(
    List<Guid> Ids,
    string? LanguageCode = null
);