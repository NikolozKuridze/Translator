using MediatR;

namespace Translator.Application.Features.Migrations.Commands.SeedFakeData;

public record SeedFakeDataCommand() : IRequest;