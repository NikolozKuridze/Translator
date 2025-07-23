namespace Translator.Application.Exceptions;

public abstract class ApplicationLayerException : Exception
{
    public ErrorCodes ErrorCode { get; }

    public ApplicationLayerException(ErrorCodes errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}