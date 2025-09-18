using System.Security.Cryptography;
using System.Text;

namespace Translator.Domain.Entities;

public class Template : BaseEntity
{
    private const int HashMaxLength = 24;
    private readonly List<Value> _values = new();

    public Template(string name, Guid? ownerId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Hash = HashName(name);
        OwnerId = ownerId;
    }

    public string Name { get; private set; }
    public string Hash { get; private set; }
    public Guid? OwnerId { get; set; }
    public User? Owner { get; set; }
    public ICollection<Value> Values => _values.AsReadOnly();

    public bool IsGlobal => OwnerId == null;

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
        if (!_values.Contains(value)) _values.Add(value);
    }

    public void RemoveValue(Value value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        _values.Remove(value);
    }
}