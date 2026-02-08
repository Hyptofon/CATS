using Domain.Products;

namespace Infrastructure.Persistence.Extensions;

public static class ProductQueryExtensions
{
    public static IQueryable<Product> WithSearchTerm(
        this IQueryable<Product> query, 
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;
        
        var terms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var term in terms)
        {
            var lowerTerm = term.ToLower(); 

            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerTerm) || 
                (p.Description != null && p.Description.ToLower().Contains(lowerTerm))
            );
        }

        return query;
    }
    
    public static IQueryable<Product> WithProductType(
        this IQueryable<Product> query, 
        int? productTypeId)
    {
        if (!productTypeId.HasValue)
            return query;

        return query.Where(p => p.ProductTypeId == productTypeId.Value);
    }
}
