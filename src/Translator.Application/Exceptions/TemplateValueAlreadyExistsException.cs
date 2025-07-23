namespace Translator.Application.Exceptions;

public class TemplateValueAlreadyExistsException : ApplicationLayerException
{
    public TemplateValueAlreadyExistsException(string key) 
        : base(ErrorCodes.BadRequest, $"Template value with key {key} already exists") { }
}