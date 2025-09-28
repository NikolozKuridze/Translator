namespace Translator.API.Models;

public sealed record AddValueToTemplateRequest(
    Guid TemplateId,
    string ValueName);