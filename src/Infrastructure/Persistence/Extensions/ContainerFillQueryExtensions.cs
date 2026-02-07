using Domain.Containers;

namespace Infrastructure.Persistence.Extensions;

public static class ContainerFillQueryExtensions
{
    public static IQueryable<ContainerFill> WithProductId(
        this IQueryable<ContainerFill> query, 
        int? productId)
    {
        if (!productId.HasValue)
            return query;
        
        return query.Where(f => f.ProductId == productId.Value);
    }

    public static IQueryable<ContainerFill> WithProductTypeId(
        this IQueryable<ContainerFill> query, 
        int? productTypeId)
    {
        if (!productTypeId.HasValue)
            return query;
        
        return query.Where(f => f.Product != null && f.Product.ProductTypeId == productTypeId.Value);
    }

    public static IQueryable<ContainerFill> WithContainerIdFilter(
        this IQueryable<ContainerFill> query, 
        int? containerId)
    {
        if (!containerId.HasValue)
            return query;
        
        return query.Where(f => f.ContainerId == containerId.Value);
    }

    public static IQueryable<ContainerFill> WithDateRange(
        this IQueryable<ContainerFill> query, 
        DateTime? fromDate,
        DateTime? toDate)
    {
        if (fromDate.HasValue)
        {
            query = query.Where(f => f.FilledDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(f => f.FilledDate <= toDate.Value);
        }

        return query;
    }

    public static IQueryable<ContainerFill> WithOnlyActive(
        this IQueryable<ContainerFill> query, 
        bool? onlyActive)
    {
        if (!onlyActive.HasValue)
            return query;

        if (onlyActive.Value)
        {
            return query.Where(f => f.EmptiedDate == null);
        }

        return query;
    }
}
