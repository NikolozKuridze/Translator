namespace Translator.API.Models;

public sealed record GetTemplatesWithIdsModel(
    List<Guid> TemplateIds,
    string? LanguageCode = null
);