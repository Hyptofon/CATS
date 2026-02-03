using Domain.Containers;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IContainerRepository : IBaseRepository<Container>
{
    Task<Option<Container>> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<Option<Container>> GetByIdAsync(int id, CancellationToken cancellationToken);
}