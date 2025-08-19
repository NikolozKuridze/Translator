using MediatR;

namespace Translator.Application.Features.Migrations.Commands.SeedLanguages;

public record SeedLanguagesCommand() : IRequest;