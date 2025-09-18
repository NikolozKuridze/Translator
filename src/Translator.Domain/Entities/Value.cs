namespace Translator.Domain.Entities;

public class Value : BaseEntity
{
    public Guid? TemplateId { get; set; }
    public string Key { get; private set; }
    public string Hash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    private List<Template> _templates = [];
    public ICollection<Template> Templates => _templates.AsReadOnly();
    
    private List<Translation> _translations = [];
    public ICollection<Translation> Translations => _translations.AsReadOnly();

    public Value(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Hash = Template.HashName(key);
        CreatedAt = DateTimeOffset.UtcNow;
    }
}