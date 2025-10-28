namespace Translator.Application.Contracts;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    Guid? GetCurrentUserId();
    string? GetCurrentUserName();
}