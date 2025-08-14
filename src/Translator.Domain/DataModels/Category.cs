namespace Translator.Domain.DataModels;

public class Category(string value, string type, int? order = null, Guid? parentId = null) : BaseDataModel
{
    public Guid? ParentId { get; init; } = parentId;
    public string Value { get; private set; } = value;
    public int? Order { get; private set; } = order;
    public string Type { get; private set; } = type;
    
    public Category? Parent { get; set; }
    public List<Category>? Children { get; set; }
}