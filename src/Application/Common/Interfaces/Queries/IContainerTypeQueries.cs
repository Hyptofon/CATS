using Domain.ContainerTypes;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IContainerTypeQueries
{
    Task<Option<ContainerType>> GetByIdAsync(ContainerTypeId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContainerType>> GetAllAsync(CancellationToken cancellationToken);
}