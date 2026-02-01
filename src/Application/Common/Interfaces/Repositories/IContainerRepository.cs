using Domain.Containers;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IContainerRepository
{
    void Add(Container container);
    void Update(Container container);
    void Delete(Container container);
    Task<Option<Container>> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<Option<Container>> GetByIdAsync(ContainerId id, CancellationToken cancellationToken);
}