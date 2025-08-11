namespace Translator.Domain.DataModels;

public class Value : BaseDataModel
{
    public Guid? TemplateId { get; set; }
    public string Key { get; private set; } = null!;
    public string Hash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    
    public ICollection<Template>? Templates  { get; set; }
    public ICollection<Translation> Translations { get; set; }

    public Value(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Hash = Template.HashName(key);
        CreatedAt = DateTimeOffset.UtcNow;
    }
}