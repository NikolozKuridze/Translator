namespace Translator.API.Models;

public record CreateCategoryModel(
    string Value,
    string Type,
    int? Order,
    Guid? ParentId);