namespace Translator.Application.Exceptions;

public class LanguageNotFoundException : ApplicationLayerException
{
    public LanguageNotFoundException(string code) 
        : base(ErrorCodes.NotFound, $"Language with code {code} not supported.") { }
}