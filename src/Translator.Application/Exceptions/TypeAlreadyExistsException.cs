namespace Translator.Application.Exceptions;

public class TypeAlreadyExistsException : ApplicationLayerException
{
    public TypeAlreadyExistsException(string typeName) 
        : base(ErrorCodes.BadRequest, $"Type '{typeName}' already exists.") { }
}