namespace Translator.Domain.Entities;

public class User(string userName) : BaseEntity
{
    public string Username { get; set; } = userName;
    public string SecretKey { get; set; } =  string.Empty;
    public IEnumerable<Template> Templates { get; set; } = new List<Template>();
    public IEnumerable<Value> Values { get; set; } = new List<Value>();
}