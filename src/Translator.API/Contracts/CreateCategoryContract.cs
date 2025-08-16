namespace Translator.API.Contracts;

public record CreateCategoryContract(
    string Value,
    string Type,
    int? Order,
    Guid? ParentId);