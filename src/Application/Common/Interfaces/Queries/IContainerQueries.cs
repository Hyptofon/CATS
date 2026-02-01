using Domain.Containers;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IContainerQueries
{
    Task<IReadOnlyList<Container>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Container>> SearchAsync(
        string? searchTerm,
        Guid? containerTypeId,
        string? status,
        CancellationToken cancellationToken);
    Task<Option<Container>> GetByIdAsync(ContainerId id, CancellationToken cancellationToken);
    Task<Option<Container>> GetByCodeAsync(string code, CancellationToken cancellationToken);
}