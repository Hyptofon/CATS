using Domain.Containers;
using MediatR;

namespace Application.Containers.Queries.SearchContainerFills;

public record SearchContainerFillsQuery(
    int? ProductId,
    int? ProductTypeId,
    int? ContainerId,
    DateTime? FromDate,
    DateTime? ToDate,
    bool? OnlyActive
) : IRequest<IReadOnlyList<ContainerFill>>;
