namespace Translator.Application.Exceptions;

public class CategoryNotFoundException : ApplicationLayerException
{
    public CategoryNotFoundException(string id)
        : base(ErrorCodes.NotFound, $"Category '{id}' was not found.") { }
}