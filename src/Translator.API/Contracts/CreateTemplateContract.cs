namespace Translator.API.Contracts;

public record CreateTemplateContract(string TemplateName, IList<string> Values);