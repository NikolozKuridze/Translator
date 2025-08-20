namespace Translator.API.Contracts;

public record UpdateCategoryContract(
    Guid Id,
    string? Value,
    int? Order);