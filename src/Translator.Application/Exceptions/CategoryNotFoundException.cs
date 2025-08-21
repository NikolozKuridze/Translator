namespace Translator.Application.Exceptions;

public class CategoryNotFoundException : ApplicationLayerException
{
    public CategoryNotFoundException(Guid? id)
        : base(ErrorCodes.NotFound, $"Category '{id}' was not found.") { }
}