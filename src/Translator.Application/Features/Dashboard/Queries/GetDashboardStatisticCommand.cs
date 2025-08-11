using MediatR;

namespace Translator.Application.Features.Dashboard.Queries;

public record GetDashboardStatisticCommand() : IRequest<GetDashboardRequestResponse>;