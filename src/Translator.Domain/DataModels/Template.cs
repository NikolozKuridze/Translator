using System.Security.Cryptography;
using System.Text;

namespace Translator.Domain.DataModels;

public class Template : BaseDataModel
{
    private readonly Lazy<SHA256> _hasher = new(SHA256.Create);
    public string Name { get; private set; }
    public string? Hash { get; private set; }
    
    public ICollection<TemplateValue> TemplateValues { get; set; }

    public Template(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Hash = _hasher.Value.ComputeHash(Encoding.UTF8.GetBytes(name)).ToString();
        TemplateValues = new List<TemplateValue>();
    }
}