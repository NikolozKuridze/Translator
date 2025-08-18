namespace Translator.API.Contracts;

public record UpdateCategoryContract(
    Guid Id,
    string? Type,
    string? Value,
    int? Order,
    Guid? ParentId);