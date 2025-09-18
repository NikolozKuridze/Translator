namespace Translator.Application.Exceptions;

public class UnauthorizedOperationException : ApplicationLayerException
{
    public UnauthorizedOperationException(string message)
        : base(ErrorCodes.Unauthorized, message)
    {
    }
}