using System.Security.Cryptography;
using System.Text;

namespace Translator.Infrastructure;

public static class StaticGuidGenerator
{
    public static Guid CreateGuidFromString(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}