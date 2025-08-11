namespace Translator.Domain;

public abstract class BaseDataModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
}