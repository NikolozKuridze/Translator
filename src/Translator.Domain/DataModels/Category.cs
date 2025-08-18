namespace Translator.Domain.DataModels;

public class Category(
    string value,
    string type, 
    int? order = null,
    Guid? parentId = null) : BaseDataModel
{
    public string Value { get; set; } = value;
    public string Type { get; set; } = type;
    public int? Order { get; set; } = order;
    public Guid? ParentId { get; set; } = parentId;
    
    public Category? Parent { get; set; }
    public List<Category>? Children { get; set; }
}