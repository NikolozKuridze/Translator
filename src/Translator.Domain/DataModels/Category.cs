namespace Translator.Domain.DataModels;

public class Category(
    string value,
    Guid typeId,
    int? order = null,
    Guid? parentId = null) : BaseDataModel
{
    public string Value { get; set; } = value;
    public Guid TypeId { get; init; } = typeId;
    public CategoryType? Type { get; init; }
    public int? Order { get; set; } = order;
    public Guid? ParentId { get; init; } = parentId;
    public Category? Parent { get; init; }
    public List<Category>? Children { get; init; }
}