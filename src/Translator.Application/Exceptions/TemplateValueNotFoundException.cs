namespace Translator.Application.Exceptions;

public class TemplateValueNotFoundException : ApplicationLayerException
{
    public TemplateValueNotFoundException(string templateValueName)
        : base(ErrorCodes.NotFound, $"template value '{templateValueName}' not found") { }
}