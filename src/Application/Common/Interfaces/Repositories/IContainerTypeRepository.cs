using Domain.ContainerTypes;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IContainerTypeRepository
{
    void Add(ContainerType containerType);
    void Update(ContainerType containerType);
    void Delete(ContainerType containerType);
    Task<Option<ContainerType>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<Option<ContainerType>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> HasContainersAsync(int id, CancellationToken cancellationToken);
}