using Translator.Domain.Enums;

namespace Translator.API.Contracts;

public record CreateTranslationContract(
        string Value,
        Languages Language
    );