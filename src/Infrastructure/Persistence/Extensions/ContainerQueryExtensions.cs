using Domain.Containers;

namespace Infrastructure.Persistence.Extensions;

public static class ContainerQueryExtensions
{
    public static IQueryable<Container> WithSearchTerm(
        this IQueryable<Container> query, 
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;
        
        var terms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var term in terms)
        {
            var lowerTerm = term.ToLower(); 

            query = query.Where(x => 
                x.Name.ToLower().Contains(lowerTerm) || 
                x.Code.ToLower().Contains(lowerTerm) ||
                (x.Meta != null && Convert.ToString(x.Meta).ToLower().Contains(lowerTerm))
            );
        }

        return query;
    }
    
    public static IQueryable<Container> WithContainerType(
        this IQueryable<Container> query, 
        int? containerTypeId)
    {
        if (!containerTypeId.HasValue)
            return query;

        return query.Where(c => c.ContainerTypeId == containerTypeId.Value);
    }

    public static IQueryable<Container> WithStatus(
        this IQueryable<Container> query, 
        string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return query;

        if (!Enum.TryParse<ContainerStatus>(status, true, out var statusEnum))
            return query;

        return query.Where(x => x.Status == statusEnum);
    }

    public static IQueryable<Container> WithProductionDate(
        this IQueryable<Container> query, 
        DateTime? productionDate)
    {
        if (!productionDate.HasValue)
            return query;
        
        return query.Where(c => c.CurrentProductionDate.HasValue 
            && c.CurrentProductionDate.Value.Date == productionDate.Value.Date);
    }

    public static IQueryable<Container> WithCurrentProduct(
        this IQueryable<Container> query, 
        int? currentProductId)
    {
        if (!currentProductId.HasValue)
            return query;
        
        return query.Where(c => c.Status == ContainerStatus.Full 
            && c.CurrentProductId == currentProductId.Value);
    }

    public static IQueryable<Container> WithLastProduct(
        this IQueryable<Container> query, 
        int? lastProductId)
    {
        if (!lastProductId.HasValue)
            return query;
        
        return query.Where(c => c.Status == ContainerStatus.Empty 
            && c.LastProductId == lastProductId.Value);
    }

    public static IQueryable<Container> WithCurrentProductType(
        this IQueryable<Container> query, 
        int? currentProductTypeId)
    {
        if (!currentProductTypeId.HasValue)
            return query;
        
        return query.Where(c => c.Status == ContainerStatus.Full 
            && c.CurrentProductTypeId == currentProductTypeId.Value);
    }

    public static IQueryable<Container> WithExpiration(
        this IQueryable<Container> query, 
        bool? showExpired)
    {
        if (!showExpired.HasValue)
            return query;
        
        if (showExpired.Value)
            return query.Where(c => c.Status == ContainerStatus.Full 
                && c.CurrentExpirationDate.HasValue 
                && c.CurrentExpirationDate.Value < DateTime.UtcNow);
        
        return query;
    }
}