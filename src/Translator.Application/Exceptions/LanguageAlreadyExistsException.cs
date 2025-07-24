namespace Translator.Application.Exceptions;

public class LanguageAlreadyExistsException : ApplicationLayerException
{
    public LanguageAlreadyExistsException(string language) 
        : base(ErrorCodes.BadRequest, $"language {language} already exists.") { }
}