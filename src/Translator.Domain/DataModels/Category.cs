namespace Translator.Domain.DataModels;

public class Category(
    string value,
    Guid typeId,
    int? order = null,
    Guid? parentId = null) : BaseDataModel
{
    public string Value { get; set; } = value;
    public Guid TypeId { get; init; } = typeId;
    public CategoryType Type { get; set; }
    public int? Order { get; set; } = order;
    public Guid? ParentId { get; set; } = parentId;
    public Category? Parent { get; set; }
    public List<Category>? Children { get; set; }
}