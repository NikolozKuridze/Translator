namespace Translator.API.Models;

public record CreateCategoryModel(
    string Value,
    string Type,
    string? Metadata,
    string? Shortcode,
    int? Order,
    Guid? ParentId);