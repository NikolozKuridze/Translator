namespace Translator.Application.Contracts.Infrastructure;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    Guid? GetCurrentUserId();
    string? GetCurrentUserName();
}