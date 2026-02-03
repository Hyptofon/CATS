using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Containers;
using Infrastructure.Persistence.Extensions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ContainerRepository(ApplicationDbContext context) 
    : BaseRepository<Container>(context), IContainerRepository, IContainerQueries
{
    public async Task<Option<Container>> GetByCodeAsync(
        string code, 
        CancellationToken cancellationToken)
    {
        var entity = await context.Containers
            .Include(x => x.ContainerType)
            .FirstOrDefaultAsync(
                x => x.Code.ToLower() == code.ToLower() && !x.IsDeleted, 
                cancellationToken);

        return entity ?? Option<Container>.None;
    }

    public async Task<Option<Container>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var entity = await context.Containers
            .Include(x => x.ContainerType)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return entity ?? Option<Container>.None;
    }

    public async Task<IReadOnlyList<Container>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await context.Containers
            .Include(x => x.ContainerType)
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Container>> SearchAsync(
        string? searchTerm,
        int? containerTypeId,
        string? status,
        CancellationToken cancellationToken)
    {
        return await context.Containers
            .AsNoTracking()
            .Include(x => x.ContainerType)
            .Where(x => !x.IsDeleted)
            .WithSearchTerm(searchTerm)
            .WithContainerType(containerTypeId)
            .WithStatus(status)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}