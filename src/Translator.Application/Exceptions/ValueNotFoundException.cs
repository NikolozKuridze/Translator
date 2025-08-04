namespace Translator.Application.Exceptions;

public class ValueNotFoundException : ApplicationLayerException
{
    public ValueNotFoundException(string templateValueName)
        : base(ErrorCodes.NotFound, $"template value '{templateValueName}' not found") { }
}