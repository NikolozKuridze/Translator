namespace Translator.Domain.DataModels;

public class Category : BaseDataModel
{
    public Guid? ParentId { get; init; }
    public string Value { get; private set; }
    public int Order { get; private set; }
    public string Type { get; private set; }
    
    public Category? Parent { get; private set; }
    public List<Category>? Children { get; private set; } = new();
}