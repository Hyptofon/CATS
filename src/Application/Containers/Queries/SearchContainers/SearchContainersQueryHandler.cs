using Application.Common.Interfaces.Queries;
using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries.SearchContainers;

public class SearchContainersQueryHandler(IContainerQueries containerQueries) 
    : IRequestHandler<SearchContainersQuery, IReadOnlyList<Container>>
{
    public async Task<IReadOnlyList<Container>> Handle(
        SearchContainersQuery request, 
        CancellationToken cancellationToken)
    {
        return await containerQueries.SearchAsync(
            request.SearchTerm,
            request.ContainerTypeId,
            request.Status,
            cancellationToken);
    }
}