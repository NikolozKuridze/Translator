    namespace Translator.Domain.DataModels;

    public class Language : BaseDataModel
    {
        public string Code { get; private set; } = string.Empty; 
        public string Name { get; private set; } = string.Empty;
        public string UnicodeRange { get; private set; } = string.Empty;
        public bool IsActive { get; set; }
        public ICollection<Translation> Translations { get; init; } = null!;

        public Language(string code, string name, string unicodeRange)
        {
            Code = code;
            Name = name;
            UnicodeRange = unicodeRange;
            IsActive = false;
        }
    }