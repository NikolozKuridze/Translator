namespace Translator.Domain.Entities;

public class Value : BaseEntity
{
    private readonly List<Template> _templates = [];

    private readonly List<Translation> _translations = [];

    public Value(string key, Guid? ownerId = null)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Hash = Template.HashName(key);
        CreatedAt = DateTimeOffset.UtcNow;
        OwnerId = ownerId;
    }

    public Guid? TemplateId { get; set; }
    public string Key { get; private set; }
    public string Hash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? OwnerId { get; set; }
    public User? Owner { get; set; }
    public ICollection<Template> Templates => _templates.AsReadOnly();
    public ICollection<Translation> Translations => _translations.AsReadOnly();

    public bool IsGlobal => OwnerId == null;
}