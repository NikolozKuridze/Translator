namespace Translator.Application.Exceptions;

public class LanguageMissMatchException : Exception
{
    public LanguageMissMatchException(string text, string message) 
        : base($"Language mismatch for text '{text}': {message}")
    {
    }
}
