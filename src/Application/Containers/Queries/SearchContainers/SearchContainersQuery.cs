using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries.SearchContainers;

public record SearchContainersQuery(
    string? SearchTerm,
    int? ContainerTypeId,
    string? Status,
    DateTime? ProductionDate,
    int? CurrentProductId,
    int? CurrentProductTypeId,
    int? LastProductId,
    bool? ShowExpired
) : IRequest<IReadOnlyList<Container>>;