namespace Translator.Application.Exceptions;

public class ValueAlreadyExistsException : ApplicationLayerException
{
    public ValueAlreadyExistsException(string key) 
        : base(ErrorCodes.BadRequest, $"Template value with key {key} already exists") { }
}