using System.Security.Cryptography;

namespace Translator.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string SecretKey { get; set; } =  string.Empty;
    public IEnumerable<Template> Templates { get; set; } = new List<Template>();
    public IEnumerable<Value> Values { get; set; } = new List<Value>();
    
    public User(string username)
    {
        Username = username;
        SecretKey = GenerateSecretKey();
    }

    private static string GenerateSecretKey()
    {
        const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const int length = 16;
        
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = charset[RandomNumberGenerator.GetInt32(charset.Length)];
        }
        
        return new string(result);
    }
    
    public void RegenerateSecretKey()
    {
        SecretKey = GenerateSecretKey();
    }
}