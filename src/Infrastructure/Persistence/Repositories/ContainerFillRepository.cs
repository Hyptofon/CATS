using Application.Common.Interfaces.Repositories;
using Domain.Containers;
using LanguageExt;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ContainerFillRepository(ApplicationDbContext context) 
    : BaseRepository<ContainerFill>(context), IContainerFillRepository
{
    public async Task<Option<ContainerFill>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var fill = await context.ContainerFills
            .Include(f => f.Product)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        return fill == null ? Option<ContainerFill>.None : Option<ContainerFill>.Some(fill);
    }

    public async Task<IReadOnlyList<ContainerFill>> GetByContainerIdAsync(
        int containerId,
        CancellationToken cancellationToken)
    {
        return await context.ContainerFills
            .Include(f => f.Product)
            .Where(f => f.ContainerId == containerId)
            .OrderByDescending(f => f.FilledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContainerFill>> SearchAsync(
        int? productId,
        int? productTypeId,
        int? containerId,
        DateTime? fromDate,
        DateTime? toDate,
        bool? onlyActive,
        CancellationToken cancellationToken)
    {
        return await context.ContainerFills
            .AsNoTracking()
            .Include(f => f.Product)
            .Include(f => f.Container)
            .WithProductId(productId)
            .WithProductTypeId(productTypeId)
            .WithContainerIdFilter(containerId)
            .WithDateRange(fromDate, toDate)
            .WithOnlyActive(onlyActive)
            .OrderByDescending(f => f.FilledDate)
            .ToListAsync(cancellationToken);
    }
}
