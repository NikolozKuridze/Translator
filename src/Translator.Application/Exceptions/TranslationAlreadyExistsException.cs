namespace Translator.Application.Exceptions;

public class TranslationAlreadyExistsException : ApplicationLayerException
{
    public TranslationAlreadyExistsException(string value)
        : base(ErrorCodes.BadRequest, $"translation with value {value} already exists or this language translation already added") { }
}