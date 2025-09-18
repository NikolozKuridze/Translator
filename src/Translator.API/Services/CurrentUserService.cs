using System.Security.Claims;
using Translator.Application.Contracts.Infrastructure;

namespace Translator.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId)) return null;
        return userId;
    }

    public string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;
}