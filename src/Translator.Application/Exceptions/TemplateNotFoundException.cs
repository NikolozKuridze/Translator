namespace Translator.Application.Exceptions;

public class TemplateNotFoundException : ApplicationLayerException
{
    public TemplateNotFoundException(string templateName)
        : base(ErrorCodes.NotFound, $"Template '{templateName}' was not found.")
    {
    }

    public TemplateNotFoundException(string templateName, Guid userId)
        : base(ErrorCodes.NotFound, $"Template '{templateName}' was not found for the current user.")
    {
    }

    public TemplateNotFoundException(Guid templateId)
        : base(ErrorCodes.NotFound, $"Template with ID '{templateId}' was not found.")
    {
    }
}