namespace Translator.Application.Exceptions;

public class CategoryAlreadyExistsException : ApplicationLayerException
{
    public CategoryAlreadyExistsException() 
        : base(ErrorCodes.BadRequest, $"Category already exists in ancestor or siblings categories.") { }
}