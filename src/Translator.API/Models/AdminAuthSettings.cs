namespace Translator.API.Models;

public class AdminAuthSettings
{
    public string Password { get; set; } = string.Empty;
    public string SessionName { get; set; } = "AdminSession";
    public int SessionTimeoutInMinutes { get; set; } 
}