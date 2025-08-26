namespace Translator.API.Models;

public record UpdateCategoryModel(
    Guid Id,
    string? Value,
    string? Metadata,
    string? Shortcode,
    int? Order);