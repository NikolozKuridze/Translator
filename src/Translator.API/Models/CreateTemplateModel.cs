namespace Translator.API.Models;

public record CreateTemplateModel(string TemplateName, IList<string> Values);