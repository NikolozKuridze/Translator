namespace Translator.Application.Exceptions;

public class UserNotFoundException : ApplicationLayerException
{
    public UserNotFoundException(string userName)
        : base(ErrorCodes.NotFound, $"User '{userName}' does not exist.")
    {
    }

    public UserNotFoundException(Guid userId)
        : base(ErrorCodes.NotFound, $"User with ID '{userId}' does not exist.")
    {
    }

    public UserNotFoundException()
        : base(ErrorCodes.NotFound, "User does not exist.")
    {
    }
}