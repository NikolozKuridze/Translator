using System.Security.Cryptography;
using System.Text;

namespace Translator.Domain.DataModels;

public class Template : BaseDataModel
{
    private const int HashMaxLength = 24;
    public string Name { get; private set; }
    public string Hash { get; private set; }
    
    private List<Value> _values = new();
    public ICollection<Value> Values => _values.AsReadOnly();

    public Template(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Hash = HashName(name);
    }

    public static string HashName(string key)
    {
        var input = $"{key}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)[..HashMaxLength];
    }
    
    public void AddValue(Value value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (!_values.Contains(value))
        {
            _values.Add(value);
        }
    }
}