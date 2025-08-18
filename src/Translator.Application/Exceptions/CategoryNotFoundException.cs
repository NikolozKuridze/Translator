namespace Translator.Application.Exceptions;

public class CategoryNotFoundException : ApplicationLayerException
{
    public CategoryNotFoundException(Guid? Id)
        : base(ErrorCodes.NotFound, $"Category '{Id}' was not found.") { }
}