using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Translator.API.Models;

namespace Translator.API.Controllers;
public class AuthController : Controller
{
    private readonly AdminAuthSettings _authSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IOptions<AdminAuthSettings> authSettings, ILogger<AuthController> logger)
    {
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        try
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session check error during login");
            HttpContext.Session.Clear();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public IActionResult Login(string password, string? returnUrl = null)
    {
        if (password == _authSettings.Password)
        {
            HttpContext.Session.SetString(_authSettings.SessionName, "authenticated");
            
            _logger.LogInformation("Successful admin login from {IP}", HttpContext.Connection.RemoteIpAddress);
            TempData["SuccessMessage"] = "Successfully logged in!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        _logger.LogWarning("Failed admin login attempt from {IP}", HttpContext.Connection.RemoteIpAddress);
        TempData["ErrorMessage"] = "Invalid password";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(_authSettings.SessionName);
        _logger.LogInformation("Admin logout from {IP}", HttpContext.Connection.RemoteIpAddress);
        TempData["SuccessMessage"] = "Logged out successfully";
        return RedirectToAction("Login");
    }

    private bool IsAuthenticated()
    {
        try
        {
            var sessionValue = HttpContext.Session.GetString(_authSettings.SessionName);
            return !string.IsNullOrEmpty(sessionValue) && sessionValue == "authenticated";
        }
        catch
        {
            return false;
        }
    }
}
