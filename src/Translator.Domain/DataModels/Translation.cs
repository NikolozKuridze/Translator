using Translator.Domain.Enums;

namespace Translator.Domain.DataModels;

public class Translation : BaseDataModel
{
    public Guid TemplateValueId { get; private set; }
    public string Value { get; private set; }
    public Languages Language { get; private set; }
    public DateTimeOffset Created { get; private set; }
    public DateTimeOffset Modified { get; private set; }
    public bool IsActive { get; private set; }
    
    public TemplateValue TemplateValue { get; set; }

    public Translation(
        Guid templateValueId, string value, Languages language)
    {
        TemplateValueId = templateValueId; 
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Language = language;
        IsActive = true;
        Created = DateTimeOffset.UtcNow;
    }
}   