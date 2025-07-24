using System.Security.Cryptography;
using System.Text;

namespace Translator.Domain.DataModels;

public class Template : BaseDataModel
{
    private const int HashMaxLength = 24;
    private static readonly SHA256 _hasher = SHA256.Create();
    public string Name { get; private set; }
    public string? Hash { get; private set; }
    
    public bool IsActive { get; private set; }
    public ICollection<TemplateValue> TemplateValues { get; set; }

    public Template(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Hash = HashName(name);
        TemplateValues = new List<TemplateValue>();
        IsActive = true;
    }

    public static string HashName(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        var hash = _hasher.ComputeHash(bytes);
        return Convert
            .ToBase64String(hash)
            .ToLowerInvariant()
            [..HashMaxLength];
    }
}