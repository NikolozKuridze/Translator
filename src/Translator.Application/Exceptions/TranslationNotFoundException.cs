namespace Translator.Application.Exceptions;

public class TranslationNotFoundException : ApplicationLayerException
{
    public TranslationNotFoundException(string value) 
        : base(ErrorCodes.NotFound, $"translation {value} not found") { }
}