namespace Translator.Domain.Entities;

public class Translation : BaseEntity
{
    public Guid TemplateValueId { get; init; }
    public Guid LanguageId { get; set; }
    public string TranslationValue { get; set; }
    public DateTimeOffset Created { get; private set; }
    public Language Language { get; set; } = null!;
    public Value Value { get; set; } = null!;

    
    public Translation(
        Guid templateValueId, string translationValue)
    {
        TemplateValueId = templateValueId; 
        TranslationValue = translationValue ?? throw new ArgumentNullException(nameof(translationValue));
        Created = DateTimeOffset.UtcNow;
    }
}   