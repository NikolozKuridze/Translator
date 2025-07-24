namespace Translator.Domain.DataModels;

public class Language : BaseDataModel
{
    public string Code { get; set; } = default!; 
    public string Name { get; set; } = default!;
    public string UnicodeRange { get; set; } = default!;
    
    public ICollection<Translation> Translations { get; set; } = null!;

    public Language(string code, string name, string unicodeRange)
    {
        Code = code;
        Name = name;
        UnicodeRange = unicodeRange;
    }
}