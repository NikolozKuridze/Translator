namespace Translator.Application.Exceptions;

public class UknownLanguageException : ApplicationLayerException
{
    public UknownLanguageException(string text) 
        : base(ErrorCodes.BadRequest, $"Can't recognize {text}'s language, try again") { }
}