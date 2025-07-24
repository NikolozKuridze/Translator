namespace Translator.Domain.DataModels;

public class Language : BaseDataModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!; 
    public string Name { get; set; } = default!; 
    public string Direction { get; set; }
    public bool IsActive { get; set; } = true;
}