namespace Translator.Application.Exceptions;

public class TemplateAlreadyExistsException : ApplicationLayerException
{
    public TemplateAlreadyExistsException(string templateName) 
        : base(ErrorCodes.BadRequest, $"Template with name '{templateName}' already exists.") { }
}