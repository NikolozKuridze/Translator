namespace Translator.Application.Exceptions;

public class UnkownLanguageException : ApplicationLayerException
{
    public UnkownLanguageException(string text)
        : base(ErrorCodes.BadRequest, $"Can't recognize {text}'s language, try again")
    {
    }
}