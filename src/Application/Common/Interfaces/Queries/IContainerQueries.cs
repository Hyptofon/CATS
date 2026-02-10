using Domain.Containers;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IContainerQueries
{
    Task<IReadOnlyList<Container>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Container>> SearchAsync(
        string? searchTerm,
        int? containerTypeId,
        ContainerStatus? status,
        DateTime? productionDate,
        int? currentProductId,
        int? currentProductTypeId,
        int? lastProductId,
        bool? showExpired,
        DateTime? filledToday,
        CancellationToken cancellationToken);
    Task<Option<Container>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Option<Container>> GetByCodeAsync(string code, CancellationToken cancellationToken);
}