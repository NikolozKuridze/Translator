namespace Translator.API.Models;

public record UpdateCategoryModel(
    Guid Id,
    string? Value,
    int? Order);