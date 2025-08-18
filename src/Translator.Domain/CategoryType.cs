using Translator.Domain.DataModels;

namespace Translator.Domain;

public class CategoryType
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public ICollection<Category> Categories { get; private set; } = new List<Category>();
    
    public static CategoryType Create(string name)
    {
        return new CategoryType { Name = name };
    }
}