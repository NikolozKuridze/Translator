namespace Translator.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}