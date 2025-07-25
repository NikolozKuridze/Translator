namespace Translator.Application.Exceptions;

public class LanguageAlreadyAdded : ApplicationLayerException
{
    public LanguageAlreadyAdded(string language) 
        : base(ErrorCodes.BadRequest, $"language {language} already added.") { }
}