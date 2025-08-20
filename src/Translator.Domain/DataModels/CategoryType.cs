namespace Translator.Domain.DataModels;

public class CategoryType(string name) : BaseDataModel
{
    public string Name { get; set; } = name; 
    public List<Category> Categories { get; set; } = [];
}