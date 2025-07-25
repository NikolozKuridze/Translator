namespace Translator.Domain;

public abstract class BaseDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsActive { get; set; } = false;
}