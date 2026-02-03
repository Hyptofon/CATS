using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries.SearchContainers;

public record SearchContainersQuery(
    string? SearchTerm,
    int? ContainerTypeId,
    string? Status
) : IRequest<IReadOnlyList<Container>>;