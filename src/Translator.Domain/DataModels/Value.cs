namespace Translator.Domain.DataModels;

public class Value : BaseDataModel
{
    public Guid? TemplateId { get; set; }
    public string Key { get; private set; } = null!;
    public string Hash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    
    private List<Template>? _templates = new();
    public ICollection<Template>? Templates => _templates?.AsReadOnly();
    
    private List<Translation>? _translations = new();
    public ICollection<Translation>? Translations => _translations?.AsReadOnly();

    public Value(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Hash = Template.HashName(key);
        CreatedAt = DateTimeOffset.UtcNow;
    }
}