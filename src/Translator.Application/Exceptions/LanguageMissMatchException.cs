namespace Translator.Application.Exceptions;

public class LanguageMissMatchException : ApplicationLayerException
{
    public LanguageMissMatchException(string value, string language) 
        : base(ErrorCodes.BadRequest, $"{value}'s language doesn't match with '{language}'.") { }
}