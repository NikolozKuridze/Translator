namespace Translator.Domain.DataModels;

public class Translation : BaseDataModel
{
    public Guid TemplateValueId { get; private set; }
    public Guid LanguageId { get; set; }
    public string TranslationValue { get; private set; }
    public DateTimeOffset Created { get; private set; }
    public Language Language { get; set; }
    public Value Value { get; set; }

    private Translation() { }
    
    public Translation(
        Guid templateValueId, string value)
    {
        TemplateValueId = templateValueId; 
        TranslationValue = value ?? throw new ArgumentNullException(nameof(value));
        IsActive = true;
        Created = DateTimeOffset.UtcNow;
    }
}   