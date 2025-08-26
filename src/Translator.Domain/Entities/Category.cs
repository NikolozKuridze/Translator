namespace Translator.Domain.Entities;

public class Category(
    string value,
    Guid typeId,
    string? metadata,
    string? shortcode,
    int? order,
    Guid? parentId) : BaseEntity
{
    public string Value { get; set; } = value;
    public Guid TypeId { get; init; } = typeId;
    public int? Order { get; set; } = order;
    public string? Metadata { get; set; }
    public string? Shortcode { get; set; }
    public CategoryType Type { get; set; } = null!;
    public Guid? ParentId { get; set; } = parentId;
    public Category? Parent { get; set; }
    public List<Category>? Children { get; set; }
}