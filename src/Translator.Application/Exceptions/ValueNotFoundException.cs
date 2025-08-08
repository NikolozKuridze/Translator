namespace Translator.Application.Exceptions;

public class ValueNotFoundException : ApplicationLayerException
{
    public ValueNotFoundException(string templateValueName)
        : base(ErrorCodes.NotFound, $"value '{templateValueName}' not found") { }
}