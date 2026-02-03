using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.ContainerTypes;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ContainerTypeRepository(ApplicationDbContext context) 
    : IContainerTypeRepository, IContainerTypeQueries
{
    public void Add(ContainerType containerType)
    {
        context.ContainerTypes.Add(containerType);
    }

    public void Update(ContainerType containerType)
    {
        context.ContainerTypes.Update(containerType);
    }

    public void Delete(ContainerType containerType)
    {
        context.ContainerTypes.Remove(containerType);
    }

    public async Task<Option<ContainerType>> GetByNameAsync(
        string name, 
        CancellationToken cancellationToken)
    {
        var entity = await context.ContainerTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Name.ToLower() == name.ToLower() && !x.IsDeleted, 
                cancellationToken);

        return entity ?? Option<ContainerType>.None;
    }

    public async Task<Option<ContainerType>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var entity = await context.ContainerTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return entity ?? Option<ContainerType>.None;
    }

    public async Task<IReadOnlyList<ContainerType>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await context.ContainerTypes
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<bool> HasContainersAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        return await context.Containers
            .AnyAsync(c => c.ContainerTypeId == id && !c.IsDeleted, cancellationToken);
    }
}