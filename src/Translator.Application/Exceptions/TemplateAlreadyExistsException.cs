namespace Translator.Application.Exceptions;

public class TemplateAlreadyExistsException : ApplicationLayerException
{
    public TemplateAlreadyExistsException(Guid templateId) 
        : base(ErrorCodes.BadRequest, $"Template with Id '{templateId}' already exists.") { }
}