using Application.Common.Interfaces.Repositories;
using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries;

public record GetContainerHistoryQuery : IRequest<IReadOnlyList<ContainerFill>>
{
    public required int ContainerId { get; init; }
}

public class GetContainerHistoryQueryHandler(IContainerFillRepository containerFillRepository)
    : IRequestHandler<GetContainerHistoryQuery, IReadOnlyList<ContainerFill>>
{
    public async Task<IReadOnlyList<ContainerFill>> Handle(
        GetContainerHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await containerFillRepository.GetByContainerIdAsync(request.ContainerId, cancellationToken);
    }
}
