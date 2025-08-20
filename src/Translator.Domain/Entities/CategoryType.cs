namespace Translator.Domain.Entities;

public class CategoryType(string name) : BaseEntity
{
    public string Name { get; set; } = name; 
    public List<Category> Categories { get; set; } = [];
}