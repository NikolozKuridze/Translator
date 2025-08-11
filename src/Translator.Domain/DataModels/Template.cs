using System.Security.Cryptography;
using System.Text;

namespace Translator.Domain.DataModels;

public class Template : BaseDataModel
{
    private const int HashMaxLength = 24;
    public string Name { get; private set; }
    public string Hash { get; private set; }
    public ICollection<Value> Values { get; init; }

    public Template(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Hash = HashName(name);
        Values = new List<Value>();
    }

    public static string HashName(string key)
    {
        var input = $"{key}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)[..HashMaxLength];
    }
}