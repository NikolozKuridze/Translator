namespace Translator.Application.Exceptions;

public class TypeNotFoundException : ApplicationLayerException
{
    public TypeNotFoundException(string typeName) 
        : base(ErrorCodes.NotFound, $"Type '{typeName}' does not exist.") { }
}