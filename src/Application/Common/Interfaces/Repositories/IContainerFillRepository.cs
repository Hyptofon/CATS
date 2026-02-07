using Domain.Containers;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IContainerFillRepository : IBaseRepository<ContainerFill>
{
    Task<Option<ContainerFill>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContainerFill>> GetByContainerIdAsync(int containerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContainerFill>> SearchAsync(
        int? productId,
        int? productTypeId,
        int? containerId,
        DateTime? fromDate,
        DateTime? toDate,
        bool? onlyActive,
        CancellationToken cancellationToken);
}
