namespace Translator.API.Contracts;

public class AdminAuthSettings
{
    public string Password { get; set; } = string.Empty;
    public string SessionName { get; set; } = "AdminSession";
}