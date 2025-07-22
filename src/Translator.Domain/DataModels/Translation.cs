using Translator.Domain.Enums;

namespace Translator.Domain.DataModels;

public class Translation : BaseDataModel
{
    public Guid TemplateValueId { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }
    public Languages Language { get; private set; }
    public DateTimeOffset Created { get; private set; }
    public DateTimeOffset Modified { get; private set; }
    
    public TemplateValue TemplateValue { get; set; }

    public Translation(
        Guid templateValueId,
        string key, string value, Languages language)
    {
        TemplateValueId = templateValueId;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Language = language;
        Created = DateTimeOffset.UtcNow;
    }

    public void ModifyValue(string newValue)
    {
        Value = newValue ?? throw new ArgumentNullException(nameof(newValue));
        Modified = DateTimeOffset.UtcNow;
    }
}   