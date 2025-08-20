namespace Translator.Domain.Entities;

public class Translation : BaseEntity
{
    public Guid TemplateValueId { get; private set; }
    public Guid LanguageId { get; set; }
    public string TranslationValue { get; private set; }
    public DateTimeOffset Created { get; private set; }
    public Language Language { get; set; } = null!;
    public Value Value { get; set; } = null!;
    
    public Translation(
        Guid templateValueId, string value)
    {
        TemplateValueId = templateValueId; 
        TranslationValue = value ?? throw new ArgumentNullException(nameof(value));
        Created = DateTimeOffset.UtcNow;
    }
}   