namespace Translator.Application.Exceptions;

public class UserNotFoundException : ApplicationLayerException
{
    public UserNotFoundException(string userName) 
        : base(ErrorCodes.NotFound, $"User '{userName}' does not exist.") { }
}