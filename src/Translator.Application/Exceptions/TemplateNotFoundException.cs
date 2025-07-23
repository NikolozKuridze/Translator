namespace Translator.Application.Exceptions;

public class TemplateNotFoundException : ApplicationLayerException
{
    public TemplateNotFoundException(string templateNam) 
        : base(ErrorCodes.NotFound, $"Template '{templateNam}' was not found.") { }
}