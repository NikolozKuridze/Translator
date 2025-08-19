namespace Translator.Domain.DataModels;

public class CategoryType(string type) : BaseDataModel
{
    public string Type { get; init; } = type;
    public List<Category> Categories { get; set; } = [];
}