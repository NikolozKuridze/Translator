namespace Translator.Domain.DataModels;

public class TemplateValue : BaseDataModel
{
    public Guid TemplateId { get; private set; }
    
    public string Key { get; private set; } = null!;
    public string Hash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    
    public Template Template { get; private set; } = null!;
    public ICollection<Translation> Translations { get; set; }

    public TemplateValue(Guid templateId, string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Hash = Template.HashName(key);
        TemplateId = templateId;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}