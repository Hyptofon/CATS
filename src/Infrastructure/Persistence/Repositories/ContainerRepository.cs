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
            .Include(x => x.CurrentProduct)
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastModifiedByUser)
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
                .ThenInclude(ct => ct.AllowedProductTypes)
            .Include(x => x.CurrentProduct)
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastModifiedByUser)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        return entity ?? Option<Container>.None;
    }

    public async Task<IReadOnlyList<Container>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await context.Containers
            .Include(x => x.ContainerType)
            .Include(x => x.CurrentProduct)
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastModifiedByUser)
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Container>> SearchAsync(
        string? searchTerm,
        int? containerTypeId,
        ContainerStatus? status,
        DateTime? productionDate,
        int? currentProductId,
        int? currentProductTypeId,
        int? lastProductId,
        bool? showExpired,
        DateTime? filledToday,
        CancellationToken cancellationToken)
    {
        return await context.Containers
            .AsNoTracking()
            .Include(x => x.ContainerType)
            .Include(x => x.CurrentProduct)
            .Include(x => x.CreatedByUser)
            .Include(x => x.LastModifiedByUser)
            .Where(x => !x.IsDeleted)
            .WithSearchTerm(searchTerm)
            .WithContainerType(containerTypeId)
            .WithStatus(status)
            .WithProductionDate(productionDate)
            .WithProduct(currentProductId)
            .WithCurrentProductType(currentProductTypeId)
            .WithLastProduct(lastProductId)
            .WithExpiration(showExpired)
            .WithFilledToday(filledToday)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountByTypeIdAsync(
        int containerTypeId,
        CancellationToken cancellationToken)
    {
        return await context.Containers
            .CountAsync(x => x.ContainerTypeId == containerTypeId, cancellationToken);
    }
}