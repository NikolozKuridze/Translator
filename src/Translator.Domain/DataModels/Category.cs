namespace Translator.Domain.DataModels;

public class Category(string value, string type, int? order = null, Guid? parentId = null) : BaseDataModel
{
    public string Value { get; private set; } = value;
    public string Type { get; private set; } = type;
    public int? Order { get; set; } = order;
    public Guid? ParentId { get; init; } = parentId;
    
    public Category? Parent { get; set; }
    public List<Category>? Children { get; set; }
}