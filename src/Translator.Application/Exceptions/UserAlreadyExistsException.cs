namespace Translator.Application.Exceptions;

public class UserAlreadyExistsException : ApplicationLayerException
{
    public UserAlreadyExistsException(string userName) 
        : base(ErrorCodes.BadRequest, $"User '{userName}' already exists.") { }
}