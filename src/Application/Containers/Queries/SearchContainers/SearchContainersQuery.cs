using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries.SearchContainers;

public record SearchContainersQuery(
    string? SearchTerm,
    int? ContainerTypeId,
    ContainerStatus? Status,
    DateTime? ProductionDate,
    int? CurrentProductId,
    int? CurrentProductTypeId,
    int? LastProductId,
    bool? ShowExpired,
    DateTime? FilledToday
) : IRequest<IReadOnlyList<Container>>;