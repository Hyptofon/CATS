using Application.Common.Interfaces.Repositories;
using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries.SearchContainerFills;

public class SearchContainerFillsQueryHandler(IContainerFillRepository containerFillRepository) 
    : IRequestHandler<SearchContainerFillsQuery, IReadOnlyList<ContainerFill>>
{
    public async Task<IReadOnlyList<ContainerFill>> Handle(
        SearchContainerFillsQuery request, 
        CancellationToken cancellationToken)
    {
        return await containerFillRepository.SearchAsync(
            request.ProductId,
            request.ProductTypeId,
            request.ContainerId,
            request.FromDate,
            request.ToDate,
            request.OnlyActive,
            cancellationToken);
    }
}
