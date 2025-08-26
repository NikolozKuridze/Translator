using MediatR;

namespace Translator.Application.Features.Migrations.Commands.DropDatabase;

public record DropDatabaseCommand() :  IRequest;