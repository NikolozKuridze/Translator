namespace Translator.Domain.DataModels;

public class Translation : BaseDataModel
{
    public Guid TemplateValueId { get; private set; }
    public Guid LanguageId { get; private set; }
    public string Value { get; private set; }
    public Language Language { get; set; }
    public DateTimeOffset Created { get; private set; }
    
    public TemplateValue TemplateValue { get; set; }

    public Translation(
        Guid templateValueId, string value)
    {
        TemplateValueId = templateValueId; 
        Value = value ?? throw new ArgumentNullException(nameof(value));
        IsActive = true;
        Created = DateTimeOffset.UtcNow;
    }
}   